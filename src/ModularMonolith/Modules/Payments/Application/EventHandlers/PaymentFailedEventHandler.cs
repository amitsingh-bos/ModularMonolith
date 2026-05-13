using Microsoft.Extensions.Logging;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.Modules.Payments.Domain.Events;

namespace ModularMonolith.Modules.Payments.Application.EventHandlers;

public sealed class PaymentFailedEventHandler : IDomainEventHandler<PaymentFailedDomainEvent>
{
    private readonly ILogger<PaymentFailedEventHandler> _logger;

    public PaymentFailedEventHandler(ILogger<PaymentFailedEventHandler> logger)
        => _logger = logger;

    public Task HandleAsync(PaymentFailedDomainEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogError(
            "Payment {PaymentId} failed for order {OrderId} in tenant {TenantId}. Reason: {Reason}",
            domainEvent.PaymentId, domainEvent.OrderId, domainEvent.TenantId, domainEvent.FailureReason);

        return Task.CompletedTask;
    }
}
