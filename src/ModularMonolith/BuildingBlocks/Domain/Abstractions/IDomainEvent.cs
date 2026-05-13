namespace ModularMonolith.BuildingBlocks.Domain.Abstractions;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
