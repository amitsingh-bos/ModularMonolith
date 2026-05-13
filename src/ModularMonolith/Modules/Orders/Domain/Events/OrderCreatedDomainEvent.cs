using ModularMonolith.BuildingBlocks.Domain.Abstractions;

namespace ModularMonolith.Modules.Orders.Domain.Events;

public sealed record OrderCreatedDomainEvent(
    Guid OrderId,
    Guid TenantId,
    Guid CustomerId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
