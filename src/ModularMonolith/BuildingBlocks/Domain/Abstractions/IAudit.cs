namespace ModularMonolith.BuildingBlocks.Domain.Abstractions;

/// <summary>
/// Marker interface. Repositories that implement this will have Add/Update/Delete operations
/// automatically tracked by IAuditLogger and persisted to the audit_logs table.
/// </summary>
public interface IAudit { }
