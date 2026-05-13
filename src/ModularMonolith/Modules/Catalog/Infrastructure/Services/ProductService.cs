using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.BuildingBlocks.Domain.Exceptions;
using ModularMonolith.Modules.Catalog.Application.DTOs;
using ModularMonolith.Modules.Catalog.Application.Services;
using ModularMonolith.Modules.Catalog.Domain.Entities;
using ModularMonolith.Modules.Catalog.Domain.Exceptions;
using ModularMonolith.Modules.Catalog.Domain.Repositories;
using ModularMonolith.Modules.Catalog.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Catalog.Infrastructure.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUser _currentUser;
    private readonly CatalogDbContext _context;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ICurrentUser currentUser,
        CatalogDbContext context)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
        _context = context;
    }

    public async Task<ProductDto> GetByIdAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, ct)
            ?? throw new ProductNotFoundException(productId);

        return MapToDto(product);
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(Guid tenantId, GetProductsRequest request, CancellationToken ct = default)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.TenantId == tenantId);

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (request.IsActive.HasValue)
            query = query.Where(p => p.IsActive == request.IsActive.Value);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToUpperInvariant();
            query = query.Where(p =>
                p.Name.ToUpper().Contains(term) ||
                p.Sku.Contains(term));
        }

        var total = await query.CountAsync(ct);

        var products = await query
            .OrderBy(p => p.Name)
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<ProductDto>(products.Select(MapToDto).ToList(), total, request.PageNumber, request.PageSize);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        if (await _productRepository.ExistsBySkuAsync(request.Sku, request.TenantId, ct))
            throw new DomainException($"A product with SKU '{request.Sku.ToUpperInvariant()}' already exists in this tenant.");

        if (!await _categoryRepository.ExistsByIdAsync(request.CategoryId, ct))
            throw new CategoryNotFoundException(request.CategoryId);

        var product = Product.Create(
            request.TenantId,
            request.CategoryId,
            request.Name,
            request.Description,
            request.Sku,
            request.Price,
            request.StockQuantity);

        await _productRepository.AddAsync(product, ct);
        await _productRepository.SaveChangesAsync(ct);

        var created = await _productRepository.GetByIdAsync(product.Id, ct);
        return MapToDto(created!);
    }

    public async Task<ProductDto> UpdateAsync(Guid productId, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, ct)
            ?? throw new ProductNotFoundException(productId);

        if (!await _categoryRepository.ExistsByIdAsync(request.CategoryId, ct))
            throw new CategoryNotFoundException(request.CategoryId);

        product.Update(request.Name, request.Description, request.Price, request.CategoryId);
        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync(ct);

        var updated = await _productRepository.GetByIdAsync(productId, ct);
        return MapToDto(updated!);
    }

    public async Task DeleteAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, ct)
            ?? throw new ProductNotFoundException(productId);

        product.SoftDelete(_currentUser.UserId);
        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync(ct);
    }

    public async Task AdjustStockAsync(Guid productId, AdjustStockRequest request, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, ct)
            ?? throw new ProductNotFoundException(productId);

        product.AdjustStock(request.Delta);
        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync(ct);
    }

    private static ProductDto MapToDto(Product p) => new(
        p.Id,
        p.TenantId,
        p.CategoryId,
        p.Category?.Name ?? string.Empty,
        p.Name,
        p.Description,
        p.Sku,
        p.Price,
        p.StockQuantity,
        p.IsActive,
        p.CreatedAt,
        p.UpdatedAt);
}
