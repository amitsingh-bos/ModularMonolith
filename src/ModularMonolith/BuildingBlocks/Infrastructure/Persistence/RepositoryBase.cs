using Microsoft.EntityFrameworkCore;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Primitives;

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

    protected virtual void UpdateEntity(TEntity entity)
    {
        var oldValues = Context.Entry(entity)
            .OriginalValues.Properties
            .ToDictionary(
                p => p.Name,
                p => Context.Entry(entity).OriginalValues[p]);

        Context.Set<TEntity>().Update(entity);

        if (_auditEnabled) AuditLogger.TrackUpdate(entity, oldValues);
    }

    protected virtual void DeleteEntity(TEntity entity)
    {
        if (_auditEnabled) AuditLogger.TrackDelete(entity);
        Context.Set<TEntity>().Remove(entity);
    }
}
