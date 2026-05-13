using ModularMonolith.BuildingBlocks.Domain.Abstractions;
using ModularMonolith.Modules.Orders.Domain.Entities;

namespace ModularMonolith.Modules.Orders.Domain.Events;

public sealed record OrderStatusChangedDomainEvent(
    Guid OrderId,
    Guid TenantId,
    OrderStatus OldStatus,
    OrderStatus NewStatus) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
