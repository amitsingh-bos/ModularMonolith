using ModularMonolith.BuildingBlocks.Domain.Abstractions;

namespace ModularMonolith.Modules.Auth.Domain.Events;

public sealed record UserLoggedInDomainEvent(
    Guid UserId,
    Guid TenantId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
