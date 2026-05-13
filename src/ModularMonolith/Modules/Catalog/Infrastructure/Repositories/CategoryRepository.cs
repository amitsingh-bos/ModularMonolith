using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence;
using ModularMonolith.Modules.Catalog.Domain.Entities;
using ModularMonolith.Modules.Catalog.Domain.Repositories;
using ModularMonolith.Modules.Catalog.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Catalog.Infrastructure.Repositories;

public sealed class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
{
    private readonly CatalogDbContext _context;

    public CategoryRepository(CatalogDbContext context, IAuditLogger auditLogger)
        : base(context, auditLogger)
    {
        _context = context;
    }

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _context.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<IReadOnlyList<Category>> GetAllAsync(Guid tenantId, CancellationToken ct) =>
        _context.Categories
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.Name)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<Category>)t.Result, ct);

    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct) =>
        _context.Categories.AnyAsync(c => c.Id == id, ct);

    public async Task AddAsync(Category category, CancellationToken ct) =>
        await AddEntityAsync(category, ct);

    public void Update(Category category) => UpdateEntity(category);

    public Task<int> SaveChangesAsync(CancellationToken ct) =>
        _context.SaveChangesAsync(ct);
}
