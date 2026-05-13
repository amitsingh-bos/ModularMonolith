using ModularMonolith.BuildingBlocks.Domain.Abstractions;

namespace ModularMonolith.Modules.Catalog.Domain.Events;

public sealed record ProductPriceChangedDomainEvent(
    Guid ProductId,
    Guid TenantId,
    decimal OldPrice,
    decimal NewPrice) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
