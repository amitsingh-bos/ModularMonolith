using ModularMonolith.BuildingBlocks.Domain.Abstractions;

namespace ModularMonolith.Modules.Payments.Domain.Events;

public sealed record PaymentFailedDomainEvent(
    Guid PaymentId,
    Guid TenantId,
    Guid OrderId,
    string FailureReason) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
