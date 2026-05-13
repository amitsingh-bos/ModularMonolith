namespace ModularMonolith.BuildingBlocks.Domain.Abstractions;

public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
    Guid? DeletedBy { get; }
}
