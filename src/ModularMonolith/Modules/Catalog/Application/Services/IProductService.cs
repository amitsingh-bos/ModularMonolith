using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.Modules.Catalog.Application.DTOs;

namespace ModularMonolith.Modules.Catalog.Application.Services;

public interface IProductService
{
    Task<ProductDto> GetByIdAsync(Guid productId, CancellationToken ct = default);
    Task<PagedResult<ProductDto>> GetProductsAsync(Guid tenantId, GetProductsRequest request, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(Guid productId, UpdateProductRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid productId, CancellationToken ct = default);
    Task AdjustStockAsync(Guid productId, AdjustStockRequest request, CancellationToken ct = default);
}
