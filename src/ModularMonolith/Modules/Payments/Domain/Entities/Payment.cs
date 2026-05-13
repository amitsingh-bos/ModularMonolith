using ModularMonolith.BuildingBlocks.Domain.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Exceptions;
using ModularMonolith.BuildingBlocks.Domain.Primitives;
using ModularMonolith.Modules.Payments.Domain.Enums;
using ModularMonolith.Modules.Payments.Domain.Events;
using ModularMonolith.Modules.Payments.Domain.Exceptions;

namespace ModularMonolith.Modules.Payments.Domain.Entities;

public sealed class Payment : AggregateRoot, IAuditableEntity
{
    private Payment() { }

    public Guid TenantId { get; private set; }
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? TransactionReference { get; private set; }
    public string? GatewayResponse { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    public decimal? RefundedAmount { get; private set; }
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public static Payment Create(
        Guid tenantId,
        Guid orderId,
        decimal amount,
        string currency,
        PaymentMethod method,
        string? transactionReference,
        string? notes)
    {
        if (amount <= 0)
            throw new DomainException("Payment amount must be greater than zero.");

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderId = orderId,
            Amount = amount,
            Currency = currency,
            Method = method,
            Status = PaymentStatus.Pending,
            TransactionReference = transactionReference,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        payment.RaiseDomainEvent(new PaymentInitiatedDomainEvent(payment.Id, tenantId, orderId, amount));
        return payment;
    }

    public void Complete(string? transactionReference, string? gatewayResponse)
    {
        if (Status != PaymentStatus.Pending)
            throw new PaymentInvalidStateException(Id, "completed", Status);

        Status = PaymentStatus.Completed;
        TransactionReference = transactionReference ?? TransactionReference;
        GatewayResponse = gatewayResponse;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PaymentCompletedDomainEvent(Id, TenantId, OrderId, Amount));
    }

    public void Fail(string failureReason, string? gatewayResponse)
    {
        if (Status != PaymentStatus.Pending)
            throw new PaymentInvalidStateException(Id, "failed", Status);

        Status = PaymentStatus.Failed;
        FailureReason = failureReason;
        GatewayResponse = gatewayResponse;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PaymentFailedDomainEvent(Id, TenantId, OrderId, failureReason));
    }

    public void Refund(decimal refundAmount, string? notes)
    {
        if (Status != PaymentStatus.Completed)
            throw new PaymentInvalidStateException(Id, "refunded", Status);

        if (refundAmount > Amount)
            throw new DomainException($"Refund amount '{refundAmount}' cannot exceed the original payment amount '{Amount}'.");

        Status = PaymentStatus.Refunded;
        RefundedAmount = refundAmount;
        RefundedAt = DateTime.UtcNow;
        Notes = notes ?? Notes;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PaymentRefundedDomainEvent(Id, TenantId, OrderId, refundAmount));
    }
}
