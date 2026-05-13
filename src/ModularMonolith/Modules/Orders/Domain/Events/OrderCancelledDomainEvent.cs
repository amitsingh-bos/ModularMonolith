using ModularMonolith.BuildingBlocks.Domain.Abstractions;

namespace ModularMonolith.Modules.Orders.Domain.Events;

public sealed record OrderCancelledDomainEvent(
    Guid OrderId,
    Guid TenantId,
    string? Reason) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
