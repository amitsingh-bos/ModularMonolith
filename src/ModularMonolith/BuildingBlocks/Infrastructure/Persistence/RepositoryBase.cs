using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Primitives;
using IVersionedEntity = ModularMonolith.BuildingBlocks.Domain.Abstractions.IVersionedEntity;

namespace ModularMonolith.BuildingBlocks.Infrastructure.Persistence;

public abstract class RepositoryBase<TEntity> where TEntity : Entity
{
    private readonly bool _auditEnabled;

    protected readonly DbContext Context;
    protected readonly IAuditLogger AuditLogger;

    protected RepositoryBase(DbContext context, IAuditLogger auditLogger)
    {
        Context = context;
        AuditLogger = auditLogger;
        _auditEnabled = this is IAudit;
    }

    protected virtual async Task AddEntityAsync(TEntity entity, CancellationToken ct)
    {
        await Context.Set<TEntity>().AddAsync(entity, ct);
        if (_auditEnabled) AuditLogger.TrackCreate(entity);
    }

    protected virtual void UpdateEntity(TEntity entity, int? expectedVersion = null)
    {
        var entry = Context.Entry(entity);

        var oldValues = entry.OriginalValues.Properties
            .ToDictionary(p => p.Name, p => entry.OriginalValues[p]);

        Context.Set<TEntity>().Update(entity);

        // When the caller supplies the version they read from the client request,
        // override the tracked OriginalValue so EF's WHERE clause uses that version.
        // If the DB row has a higher version (someone else updated it), 0 rows are
        // affected → EF throws DbUpdateConcurrencyException → caught by middleware.
        if (expectedVersion.HasValue && entity is IVersionedEntity)
            entry.Property(nameof(IVersionedEntity.Version)).OriginalValue = expectedVersion.Value;

        if (_auditEnabled) AuditLogger.TrackUpdate(entity, oldValues);
    }

    protected virtual void DeleteEntity(TEntity entity)
    {
        if (_auditEnabled) AuditLogger.TrackDelete(entity);
        Context.Set<TEntity>().Remove(entity);
    }
}
