using ModularMonolith.BuildingBlocks.Domain.Primitives;
using ModularMonolith.Modules.Auth.Domain.Entities;

namespace ModularMonolith.BuildingBlocks.Application.Abstractions;

public interface IAuditLogger
{
    void TrackCreate<T>(T entity) where T : Entity;
    void TrackUpdate<T>(T entity, IReadOnlyDictionary<string, object?> oldValues) where T : Entity;
    void TrackDelete<T>(T entity) where T : Entity;
    IReadOnlyList<AuditLog> GetPendingEntries();
    void Clear();
}
