using Microsoft.Extensions.Logging;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.Modules.Payments.Domain.Events;

namespace ModularMonolith.Modules.Payments.Application.EventHandlers;

public sealed class PaymentCompletedEventHandler : IDomainEventHandler<PaymentCompletedDomainEvent>
{
    private readonly ILogger<PaymentCompletedEventHandler> _logger;

    public PaymentCompletedEventHandler(ILogger<PaymentCompletedEventHandler> logger)
        => _logger = logger;

    public Task HandleAsync(PaymentCompletedDomainEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Payment {PaymentId} completed for order {OrderId} — amount {Amount} in tenant {TenantId}",
            domainEvent.PaymentId, domainEvent.OrderId, domainEvent.Amount, domainEvent.TenantId);

        return Task.CompletedTask;
    }
}
