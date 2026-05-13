using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Multitenancy;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence;
using ModularMonolith.Modules.Catalog.Domain.Entities;

namespace ModularMonolith.Modules.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext : BaseDbContext
{
    public CatalogDbContext(
        DbContextOptions<CatalogDbContext> options,
        ITenantContext tenantContext,
        IAuditLogger auditLogger,
        ICurrentUser currentUser)
        : base(options, tenantContext, auditLogger, currentUser) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly,
            t => t.Namespace?.StartsWith("ModularMonolith.Modules.Catalog.Infrastructure.Persistence.Configurations") == true);

        base.OnModelCreating(modelBuilder);
    }
}
