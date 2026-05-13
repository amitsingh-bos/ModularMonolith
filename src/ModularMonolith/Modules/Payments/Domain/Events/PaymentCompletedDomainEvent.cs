using ModularMonolith.BuildingBlocks.Domain.Abstractions;

namespace ModularMonolith.Modules.Payments.Domain.Events;

public sealed record PaymentCompletedDomainEvent(
    Guid PaymentId,
    Guid TenantId,
    Guid OrderId,
    decimal Amount) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
