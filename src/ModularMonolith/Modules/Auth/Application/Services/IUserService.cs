using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.Modules.Auth.Application.DTOs;

namespace ModularMonolith.Modules.Auth.Application.Services;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<PagedResult<UserDto>> GetUsersAsync(Guid tenantId, GetUsersRequest request, CancellationToken ct = default);
    Task AssignRoleAsync(AssignRoleRequest request, CancellationToken ct = default);
}
