namespace ModularMonolith.BuildingBlocks.Domain.Abstractions;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
    Guid? CreatedBy { get; }
    Guid? UpdatedBy { get; }
}
