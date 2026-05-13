using ModularMonolith.BuildingBlocks.Domain.Exceptions;

namespace ModularMonolith.Modules.Catalog.Domain.Exceptions;

public sealed class InsufficientStockException : DomainException
{
    public InsufficientStockException(Guid productId, int available, int requested)
        : base($"Insufficient stock for product '{productId}'. Available: {available}, Requested: {requested}.") { }
}
