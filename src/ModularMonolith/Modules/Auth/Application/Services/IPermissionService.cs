using ModularMonolith.Modules.Auth.Application.DTOs;

namespace ModularMonolith.Modules.Auth.Application.Services;

public interface IPermissionService
{
    Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken ct = default);
}
