using ModularMonolith.BuildingBlocks.Domain.Abstractions;

namespace ModularMonolith.Modules.Catalog.Domain.Events;

public sealed record ProductCreatedDomainEvent(Guid ProductId, Guid TenantId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
