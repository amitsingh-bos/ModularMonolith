using ModularMonolith.BuildingBlocks.Domain.Abstractions;

namespace ModularMonolith.Modules.Payments.Domain.Events;

public sealed record PaymentRefundedDomainEvent(
    Guid PaymentId,
    Guid TenantId,
    Guid OrderId,
    decimal RefundedAmount) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
