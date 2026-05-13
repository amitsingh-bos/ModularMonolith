using ModularMonolith.BuildingBlocks.Domain.Exceptions;

namespace ModularMonolith.Modules.Catalog.Domain.Exceptions;

public sealed class ProductNotFoundException : NotFoundException
{
    public ProductNotFoundException(Guid productId)
        : base("Product", productId) { }
}
