namespace ModularMonolith.BuildingBlocks.Domain.Abstractions;

public interface IVersionedEntity
{
    int Version { get; }
}
