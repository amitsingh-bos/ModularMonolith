using Microsoft.Extensions.Logging;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.Modules.Orders.Domain.Events;

namespace ModularMonolith.Modules.Orders.Application.EventHandlers;

public sealed class OrderCreatedEventHandler : IDomainEventHandler<OrderCreatedDomainEvent>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
        => _logger = logger;

    public Task HandleAsync(OrderCreatedDomainEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Order {OrderId} created for customer {CustomerId} in tenant {TenantId}",
            domainEvent.OrderId, domainEvent.CustomerId, domainEvent.TenantId);

        return Task.CompletedTask;
    }
}
