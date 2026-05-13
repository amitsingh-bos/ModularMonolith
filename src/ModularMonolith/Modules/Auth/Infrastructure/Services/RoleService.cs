using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Application.Services;
using ModularMonolith.Modules.Auth.Domain.Entities;
using ModularMonolith.Modules.Auth.Domain.Repositories;

namespace ModularMonolith.Modules.Auth.Infrastructure.Services;

public sealed class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository) => _roleRepository = roleRepository;

    public async Task<RoleDto?> GetByIdAsync(Guid roleId, CancellationToken ct = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, ct);
        return role is null ? null : MapToDto(role);
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(Guid tenantId, CancellationToken ct = default)
    {
        var roles = await _roleRepository.GetAllAsync(tenantId, ct);
        return roles.Select(MapToDto).ToList();
    }

    private static RoleDto MapToDto(Role role) => new(
        role.Id,
        role.Name,
        role.Description,
        role.RolePermissions
            .Select(rp => rp.Permission?.Code ?? string.Empty)
            .Where(c => c.Length > 0)
            .ToList());
}
