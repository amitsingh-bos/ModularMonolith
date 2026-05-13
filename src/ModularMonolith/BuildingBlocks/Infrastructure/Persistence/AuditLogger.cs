using System.Text.Json;
using ModularMonolith.BuildingBlocks.Application.Abstractions;
using ModularMonolith.BuildingBlocks.Domain.Primitives;
using ModularMonolith.Modules.Auth.Domain.Entities;

namespace ModularMonolith.BuildingBlocks.Infrastructure.Persistence;

public sealed class AuditLogger : IAuditLogger
{
    private readonly List<AuditLog> _pending = [];

    public void TrackCreate<T>(T entity) where T : Entity
    {
        var newValues = SerializeEntity(entity);
        _pending.Add(AuditLog.Create(
            tableName: typeof(T).Name,
            action: "Created",
            entityId: entity.Id.ToString(),
            oldValues: null,
            newValues: newValues,
            userId: null,
            tenantId: null));
    }

    public void TrackUpdate<T>(T entity, IReadOnlyDictionary<string, object?> oldValues) where T : Entity
    {
        _pending.Add(AuditLog.Create(
            tableName: typeof(T).Name,
            action: "Updated",
            entityId: entity.Id.ToString(),
            oldValues: JsonSerializer.Serialize(oldValues),
            newValues: SerializeEntity(entity),
            userId: null,
            tenantId: null));
    }

    public void TrackDelete<T>(T entity) where T : Entity
    {
        _pending.Add(AuditLog.Create(
            tableName: typeof(T).Name,
            action: "Deleted",
            entityId: entity.Id.ToString(),
            oldValues: SerializeEntity(entity),
            newValues: null,
            userId: null,
            tenantId: null));
    }

    public IReadOnlyList<AuditLog> GetPendingEntries() => _pending.AsReadOnly();

    public void Clear() => _pending.Clear();

    private static string SerializeEntity<T>(T entity) =>
        JsonSerializer.Serialize(entity, new JsonSerializerOptions { WriteIndented = false });
}
