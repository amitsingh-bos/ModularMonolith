using Microsoft.Extensions.Logging;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.Modules.Orders.Domain.Events;

namespace ModularMonolith.Modules.Orders.Application.EventHandlers;

public sealed class OrderStatusChangedEventHandler : IDomainEventHandler<OrderStatusChangedDomainEvent>
{
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;

    public OrderStatusChangedEventHandler(ILogger<OrderStatusChangedEventHandler> logger)
        => _logger = logger;

    public Task HandleAsync(OrderStatusChangedDomainEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Order {OrderId} transitioned {OldStatus} → {NewStatus} in tenant {TenantId}",
            domainEvent.OrderId, domainEvent.OldStatus, domainEvent.NewStatus, domainEvent.TenantId);

        return Task.CompletedTask;
    }
}
