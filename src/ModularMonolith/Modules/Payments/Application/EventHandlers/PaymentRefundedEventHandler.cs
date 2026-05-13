using Microsoft.Extensions.Logging;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.Modules.Orders.Application.Services;
using ModularMonolith.Modules.Payments.Domain.Events;

namespace ModularMonolith.Modules.Payments.Application.EventHandlers;

/// <summary>
/// Cross-module handler: when a payment is refunded, mark the corresponding
/// order as Refunded so both modules stay consistent.
///
/// This is the canonical pattern for cross-module coordination in a modular
/// monolith — modules don't call each other's services directly in the command
/// path; instead they react to each other's domain events.
/// </summary>
public sealed class PaymentRefundedEventHandler : IDomainEventHandler<PaymentRefundedDomainEvent>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<PaymentRefundedEventHandler> _logger;

    public PaymentRefundedEventHandler(
        IOrderService orderService,
        ILogger<PaymentRefundedEventHandler> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task HandleAsync(PaymentRefundedDomainEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Payment {PaymentId} refunded ({Amount}) for order {OrderId}. Marking order as Refunded.",
            domainEvent.PaymentId, domainEvent.RefundedAmount, domainEvent.OrderId);

        try
        {
            await _orderService.MarkRefundedAsync(domainEvent.OrderId, ct);
        }
        catch (Exception ex)
        {
            // The payment refund is already committed — handler failure must not roll it back.
            // In production, use an outbox or saga for guaranteed delivery.
            _logger.LogError(ex,
                "Failed to mark order {OrderId} as Refunded after payment {PaymentId} refund.",
                domainEvent.OrderId, domainEvent.PaymentId);
        }
    }
}
