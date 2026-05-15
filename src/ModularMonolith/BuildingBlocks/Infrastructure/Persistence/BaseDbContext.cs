using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Abstractions;
using ModularMonolith.BuildingBlocks.Infrastructure.Multitenancy;
using ModularMonolith.Modules.Auth.Domain.Entities;
using IVersionedEntity = ModularMonolith.BuildingBlocks.Domain.Abstractions.IVersionedEntity;

namespace ModularMonolith.BuildingBlocks.Infrastructure.Persistence;

public abstract class BaseDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;
    private readonly ICurrentUser _currentUser;
    private readonly IDomainEventDispatcher _dispatcher;

    protected BaseDbContext(
        DbContextOptions options,
        ITenantContext tenantContext,
        IAuditLogger auditLogger,
        ICurrentUser currentUser,
        IDomainEventDispatcher dispatcher) : base(options)
    {
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
        _currentUser = currentUser;
        _dispatcher = dispatcher;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditableFields();
        SetTenantId();
        IncrementVersions();
        FlushAuditEntries();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch AFTER the DB transaction commits — handler failures never
        // roll back already-persisted aggregate state.
        await DispatchDomainEventsAsync(cancellationToken);

        _auditLogger.Clear();
        return result;
    }

    private void IncrementVersions()
    {
        foreach (var entry in ChangeTracker.Entries<IVersionedEntity>()
                     .Where(e => e.State == EntityState.Modified))
        {
            var versionProp = entry.Property(nameof(IVersionedEntity.Version));
            versionProp.CurrentValue = (int)versionProp.CurrentValue! + 1;
        }
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

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var domainEvents = ChangeTracker
            .Entries<Domain.Primitives.AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        // Clear before dispatching — prevents re-dispatch if a handler triggers
        // another SaveChangesAsync on the same DbContext instance.
        foreach (var aggregate in ChangeTracker
            .Entries<Domain.Primitives.AggregateRoot>()
            .Select(e => e.Entity))
        {
            aggregate.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
            await _dispatcher.DispatchAsync(domainEvent, ct);
    }
}
