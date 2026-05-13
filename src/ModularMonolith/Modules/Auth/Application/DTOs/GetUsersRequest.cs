using ModularMonolith.BuildingBlocks.Application.Common;

namespace ModularMonolith.Modules.Auth.Application.DTOs;

public sealed class GetUsersRequest : PagedQuery
{
    public bool? IsActive { get; init; }
}
