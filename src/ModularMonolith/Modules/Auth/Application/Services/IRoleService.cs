using ModularMonolith.Modules.Auth.Application.DTOs;

namespace ModularMonolith.Modules.Auth.Application.Services;

public interface IRoleService
{
    Task<RoleDto?> GetByIdAsync(Guid roleId, CancellationToken ct = default);
    Task<IReadOnlyList<RoleDto>> GetAllAsync(Guid tenantId, CancellationToken ct = default);
}
