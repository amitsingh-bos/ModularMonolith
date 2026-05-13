using ModularMonolith.BuildingBlocks.Domain.Abstractions;

namespace ModularMonolith.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Dispatches a domain event to all registered <see cref="IDomainEventHandler{TEvent}"/>
/// implementations for that event's concrete type.
/// Called by <c>BaseDbContext</c> immediately after the DB transaction commits.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}
