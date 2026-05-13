using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.BuildingBlocks.Domain.Exceptions;
using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Application.Services;
using ModularMonolith.Modules.Auth.Domain.Entities;
using ModularMonolith.Modules.Auth.Domain.Repositories;
using ModularMonolith.Modules.Auth.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Auth.Infrastructure.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly AuthDbContext _context;

    public UserService(IUserRepository userRepository, IRoleRepository roleRepository, AuthDbContext context)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _context = context;
    }

    public async Task<UserDto> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException(nameof(User), userId);

        var roleNames = user.UserRoles
            .Select(ur => ur.Role?.Name ?? string.Empty)
            .Where(n => n.Length > 0)
            .ToList();

        return MapToDto(user, roleNames);
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(Guid tenantId, GetUsersRequest request, CancellationToken ct = default)
    {
        var query = _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => u.TenantId == tenantId);

        if (request.IsActive.HasValue)
            query = query.Where(u => u.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLowerInvariant();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term) ||
                u.Email.Value.Contains(term));
        }

        var total = await query.CountAsync(ct);

        var users = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = users.Select(u =>
        {
            var names = u.UserRoles
                .Select(ur => ur.Role?.Name ?? string.Empty)
                .Where(n => n.Length > 0)
                .ToList();
            return MapToDto(u, names);
        }).ToList();

        return new PagedResult<UserDto>(items, total, request.PageNumber, request.PageSize);
    }

    public async Task AssignRoleAsync(AssignRoleRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var roleExists = await _roleRepository.ExistsByIdAsync(request.RoleId, ct);
        if (!roleExists)
            throw new NotFoundException(nameof(Role), request.RoleId);

        user.AssignRole(request.RoleId);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);
    }

    private static UserDto MapToDto(User user, IReadOnlyList<string> roleNames) => new(
        user.Id,
        user.TenantId,
        user.Email.Value,
        user.FirstName,
        user.LastName,
        user.IsActive,
        user.IsEmailVerified,
        user.LastLoginAt,
        roleNames);
}
