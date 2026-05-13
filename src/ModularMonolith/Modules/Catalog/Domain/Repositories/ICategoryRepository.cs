using ModularMonolith.Modules.Catalog.Domain.Entities;

namespace ModularMonolith.Modules.Catalog.Domain.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetAllAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
    void Update(Category category);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
