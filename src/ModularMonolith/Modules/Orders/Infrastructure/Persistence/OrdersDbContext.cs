using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Multitenancy;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence;
using ModularMonolith.Modules.Orders.Domain.Entities;

namespace ModularMonolith.Modules.Orders.Infrastructure.Persistence;

public sealed class OrdersDbContext : BaseDbContext
{
    public OrdersDbContext(
        DbContextOptions<OrdersDbContext> options,
        ITenantContext tenantContext,
        IAuditLogger auditLogger,
        ICurrentUser currentUser)
        : base(options, tenantContext, auditLogger, currentUser) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly,
            t => t.Namespace?.StartsWith("ModularMonolith.Modules.Orders.Infrastructure.Persistence.Configurations") == true);

        base.OnModelCreating(modelBuilder);
    }
}
