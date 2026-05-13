using ModularMonolith.BuildingBlocks.Application.Common;

namespace ModularMonolith.Modules.Catalog.Application.DTOs;

public sealed class GetProductsRequest : PagedQuery
{
    public Guid? CategoryId { get; init; }
    public bool? IsActive { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
}
