using ModularMonolith.Modules.Auth.Application.DTOs;
using ModularMonolith.Modules.Auth.Application.Services;
using ModularMonolith.Modules.Auth.Domain.Repositories;

namespace ModularMonolith.Modules.Auth.Infrastructure.Services;

public sealed class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;

    public PermissionService(IPermissionRepository permissionRepository)
        => _permissionRepository = permissionRepository;

    public async Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken ct = default)
    {
        var permissions = await _permissionRepository.GetAllAsync(ct);
        return permissions
            .Select(p => new PermissionDto(p.Id, p.Code, p.Description, p.Module))
            .ToList();
    }
}
