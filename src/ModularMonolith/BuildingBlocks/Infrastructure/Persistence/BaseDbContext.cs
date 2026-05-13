using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Multitenancy;
using ModularMonolith.Modules.Auth.Domain.Entities;

namespace ModularMonolith.BuildingBlocks.Infrastructure.Persistence;

public abstract class BaseDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;
    private readonly ICurrentUser _currentUser;

    protected BaseDbContext(
        DbContextOptions options,
        ITenantContext tenantContext,
        IAuditLogger auditLogger,
        ICurrentUser currentUser) : base(options)
    {
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
        _currentUser = currentUser;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditableFields();
        SetTenantId();
        FlushAuditEntries();
        var result = await base.SaveChangesAsync(cancellationToken);
        DispatchDomainEvents();
        _auditLogger.Clear();
        return result;
    }

    private void SetAuditableFields()
    {
        var now = DateTime.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(IAuditableEntity.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(IAuditableEntity.CreatedBy)).CurrentValue = userId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(IAuditableEntity.UpdatedAt)).CurrentValue = now;
                entry.Property(nameof(IAuditableEntity.UpdatedBy)).CurrentValue = userId;
            }
        }
    }

    private void SetTenantId()
    {
        if (!_tenantContext.IsResolved) return;

        foreach (var entry in ChangeTracker.Entries()
                     .Where(e => e.State == EntityState.Added))
        {
            var tenantProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "TenantId");
            if (tenantProp is not null && tenantProp.CurrentValue is Guid g && g == Guid.Empty)
                tenantProp.CurrentValue = _tenantContext.TenantId;
        }
    }

    private void FlushAuditEntries()
    {
        var pending = _auditLogger.GetPendingEntries();
        if (pending.Count == 0) return;

        var userId = _currentUser.UserId;
        var tenantId = _tenantContext.IsResolved ? _tenantContext.TenantId : (Guid?)null;

        foreach (var entry in pending)
        {
            var enriched = AuditLog.Create(
                entry.TableName,
                entry.Action,
                entry.EntityId,
                entry.OldValues,
                entry.NewValues,
                userId,
                tenantId,
                entry.IpAddress);

            Set<AuditLog>().Add(enriched);
        }
    }

    private void DispatchDomainEvents()
    {
        var aggregates = ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<Domain.Primitives.AggregateRoot>()
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();
    }
}
