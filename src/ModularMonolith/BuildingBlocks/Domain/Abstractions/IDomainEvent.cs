namespace ModularMonolith.BuildingBlocks.Domain.Abstractions;

/// <summary>
/// Marker interface for all domain events.
/// Raised by aggregate roots during state transitions and dispatched by
/// <see cref="ModularMonolith.BuildingBlocks.Application.Abstractions.IDomainEventDispatcher"/>
/// after the database transaction commits.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
