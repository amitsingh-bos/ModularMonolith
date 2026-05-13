namespace ModularMonolith.Modules.Auth.Domain.Entities;

public sealed class AuditLog
{
    private AuditLog() { }

    public Guid Id { get; private init; }
    public string TableName { get; private init; } = string.Empty;
    public string Action { get; private init; } = string.Empty;
    public string EntityId { get; private init; } = string.Empty;
    public string? OldValues { get; private init; }
    public string? NewValues { get; private init; }
    public Guid? UserId { get; private init; }
    public Guid? TenantId { get; private init; }
    public string? IpAddress { get; private init; }
    public DateTime Timestamp { get; private init; }

    public static AuditLog Create(
        string tableName,
        string action,
        string entityId,
        string? oldValues,
        string? newValues,
        Guid? userId,
        Guid? tenantId,
        string? ipAddress = null) => new()
    {
        Id = Guid.NewGuid(),
        TableName = tableName,
        Action = action,
        EntityId = entityId,
        OldValues = oldValues,
        NewValues = newValues,
        UserId = userId,
        TenantId = tenantId,
        IpAddress = ipAddress,
        Timestamp = DateTime.UtcNow
    };
}
