using ModularMonolith.BuildingBlocks.Domain.Exceptions;

namespace ModularMonolith.Modules.Catalog.Domain.Exceptions;

public sealed class CategoryNotFoundException : NotFoundException
{
    public CategoryNotFoundException(Guid categoryId)
        : base("Category", categoryId) { }
}
