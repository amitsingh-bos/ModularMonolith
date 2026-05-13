using ModularMonolith.BuildingBlocks.Domain.Abstractions;

namespace ModularMonolith.Modules.Auth.Domain.Events;

public sealed record UserRegisteredDomainEvent(
    Guid UserId,
    Guid TenantId,
    string Email) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
