using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Multitenancy;
using ModularMonolith.BuildingBlocks.Infrastructure.Persistence;
using ModularMonolith.Modules.Payments.Domain.Entities;

namespace ModularMonolith.Modules.Payments.Infrastructure.Persistence;

public sealed class PaymentsDbContext : BaseDbContext
{
    public PaymentsDbContext(
        DbContextOptions<PaymentsDbContext> options,
        ITenantContext tenantContext,
        IAuditLogger auditLogger,
        ICurrentUser currentUser)
        : base(options, tenantContext, auditLogger, currentUser) { }

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentsDbContext).Assembly,
            t => t.Namespace?.StartsWith("ModularMonolith.Modules.Payments.Infrastructure.Persistence.Configurations") == true);

        base.OnModelCreating(modelBuilder);
    }
}
