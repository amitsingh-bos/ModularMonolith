using ModularMonolith.BuildingBlocks.Domain.Exceptions;
using ModularMonolith.Modules.Orders.Domain.Entities;

namespace ModularMonolith.Modules.Orders.Domain.Exceptions;

public sealed class OrderInvalidStatusTransitionException : DomainException
{
    public OrderInvalidStatusTransitionException(Guid orderId, OrderStatus currentStatus, OrderStatus targetStatus)
        : base($"Cannot transition order '{orderId}' from {currentStatus} to {targetStatus}.") { }
}
