using ModularMonolith.BuildingBlocks.Domain.Abstractions;

namespace ModularMonolith.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Implement this interface to handle a specific domain event.
/// All implementations are discovered at startup by scanning the assembly and
/// are registered in DI as <c>IDomainEventHandler&lt;TEvent&gt;</c> with scoped lifetime.
/// Multiple handlers for the same event are all invoked in registration order.
/// </summary>
public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}
