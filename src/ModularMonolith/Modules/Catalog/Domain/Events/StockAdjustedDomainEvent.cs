using ModularMonolith.BuildingBlocks.Domain.Abstractions;

namespace ModularMonolith.Modules.Catalog.Domain.Events;

public sealed record StockAdjustedDomainEvent(
    Guid ProductId,
    Guid TenantId,
    int NewStockQuantity) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
