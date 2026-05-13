using ModularMonolith.Modules.Catalog.Domain.Entities;

namespace ModularMonolith.Modules.Catalog.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsBySkuAsync(string sku, Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    void Update(Product product);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
