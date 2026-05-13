using ModularMonolith.BuildingBlocks.Domain.Exceptions;

namespace ModularMonolith.Modules.Orders.Domain.Exceptions;

public sealed class OrderNotFoundException : NotFoundException
{
    public OrderNotFoundException(Guid orderId)
        : base("Order", orderId) { }
}
