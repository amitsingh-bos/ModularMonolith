namespace ModularMonolith.BuildingBlocks.Application.Abstractions;

public interface ICurrentUser
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlyList<string> Permissions { get; }
    bool IsAuthenticated { get; }
}
