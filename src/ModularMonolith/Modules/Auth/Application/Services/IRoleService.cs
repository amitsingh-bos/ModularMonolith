using ModularMonolith.Modules.Auth.Application.DTOs;

namespace ModularMonolith.Modules.Auth.Application.Services;

public interface IRoleService
{
    Task<RoleDto?> GetByIdAsync(Guid roleId, CancellationToken ct = default);
    Task<IReadOnlyList<RoleDto>> GetAllAsync(Guid tenantId, CancellationToken ct = default);
    Task<RoleDto> CreateAsync(CreateRoleRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid roleId, CancellationToken ct = default);
    Task AssignPermissionAsync(Guid roleId, AssignPermissionRequest request, CancellationToken ct = default);
    Task RemovePermissionAsync(Guid roleId, Guid permissionId, CancellationToken ct = default);
}
