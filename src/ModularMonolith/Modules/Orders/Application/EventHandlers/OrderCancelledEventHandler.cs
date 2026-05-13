using Microsoft.Extensions.Logging;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.Modules.Orders.Domain.Events;

namespace ModularMonolith.Modules.Orders.Application.EventHandlers;

public sealed class OrderCancelledEventHandler : IDomainEventHandler<OrderCancelledDomainEvent>
{
    private readonly ILogger<OrderCancelledEventHandler> _logger;

    public OrderCancelledEventHandler(ILogger<OrderCancelledEventHandler> logger)
        => _logger = logger;

    public Task HandleAsync(OrderCancelledDomainEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "Order {OrderId} cancelled in tenant {TenantId}. Reason: {Reason}",
            domainEvent.OrderId, domainEvent.TenantId, domainEvent.Reason ?? "(none)");

        return Task.CompletedTask;
    }
}
