using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence;
using ModularMonolith.Modules.Catalog.Domain.Entities;
using ModularMonolith.Modules.Catalog.Domain.Repositories;
using ModularMonolith.Modules.Catalog.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Catalog.Infrastructure.Repositories;

public sealed class ProductRepository : RepositoryBase<Product>, IProductRepository
{
    private readonly CatalogDbContext _context;

    public ProductRepository(CatalogDbContext context, IAuditLogger auditLogger)
        : base(context, auditLogger)
    {
        _context = context;
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> ExistsBySkuAsync(string sku, Guid tenantId, CancellationToken ct) =>
        _context.Products.AnyAsync(
            p => p.Sku == sku.ToUpperInvariant() && p.TenantId == tenantId, ct);

    public async Task AddAsync(Product product, CancellationToken ct) =>
        await AddEntityAsync(product, ct);

    public void Update(Product product) => UpdateEntity(product);

    public Task<int> SaveChangesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}
