using ModularMonolith.BuildingBlocks.Domain.Exceptions;
using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Application.Services;
using ModularMonolith.Modules.Auth.Domain.Entities;
using ModularMonolith.Modules.Auth.Domain.Repositories;

namespace ModularMonolith.Modules.Auth.Infrastructure.Services;

public sealed class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;

    public RoleService(IRoleRepository roleRepository, IPermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
    }

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

    public async Task<RoleDto> CreateAsync(CreateRoleRequest request, CancellationToken ct = default)
    {
        var role = Role.Create(request.TenantId, request.Name, request.Description);
        await _roleRepository.AddAsync(role, ct);
        await _roleRepository.SaveChangesAsync(ct);
        return MapToDto(role);
    }

    public async Task DeleteAsync(Guid roleId, CancellationToken ct = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, ct)
            ?? throw new NotFoundException($"Role '{roleId}' not found.");

        if (role.IsSystemRole)
            throw new DomainException("System roles cannot be deleted.");

        _roleRepository.Delete(role);
        await _roleRepository.SaveChangesAsync(ct);
    }

    public async Task AssignPermissionAsync(Guid roleId, AssignPermissionRequest request, CancellationToken ct = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, ct)
            ?? throw new NotFoundException($"Role '{roleId}' not found.");

        if (!await _permissionRepository.ExistsAsync(request.PermissionId, ct))
            throw new NotFoundException($"Permission '{request.PermissionId}' not found.");

        role.AddPermission(request.PermissionId);
        await _roleRepository.SaveChangesAsync(ct);
    }

    public async Task RemovePermissionAsync(Guid roleId, Guid permissionId, CancellationToken ct = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, ct)
            ?? throw new NotFoundException($"Role '{roleId}' not found.");

        role.RemovePermission(permissionId);
        await _roleRepository.SaveChangesAsync(ct);
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
