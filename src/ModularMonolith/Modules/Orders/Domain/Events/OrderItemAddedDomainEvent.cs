using ModularMonolith.BuildingBlocks.Domain.Abstractions;

namespace ModularMonolith.Modules.Orders.Domain.Events;

public sealed record OrderItemAddedDomainEvent(
    Guid OrderId,
    Guid ProductId,
    int Quantity) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
