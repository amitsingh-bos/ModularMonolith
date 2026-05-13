using Microsoft.Extensions.Logging;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.Modules.Payments.Domain.Events;

namespace ModularMonolith.Modules.Payments.Application.EventHandlers;

public sealed class PaymentInitiatedEventHandler : IDomainEventHandler<PaymentInitiatedDomainEvent>
{
    private readonly ILogger<PaymentInitiatedEventHandler> _logger;

    public PaymentInitiatedEventHandler(ILogger<PaymentInitiatedEventHandler> logger)
        => _logger = logger;

    public Task HandleAsync(PaymentInitiatedDomainEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Payment {PaymentId} initiated for order {OrderId} — amount {Amount} in tenant {TenantId}",
            domainEvent.PaymentId, domainEvent.OrderId, domainEvent.Amount, domainEvent.TenantId);

        return Task.CompletedTask;
    }
}
