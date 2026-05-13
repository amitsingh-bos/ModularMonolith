using ModularMonolith.BuildingBlocks.Domain.Abstractions;

namespace ModularMonolith.BuildingBlocks.Domain.Primitives;

public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity(Guid id) => Id = id;
    protected Entity() { }

    public Guid Id { get; protected init; }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
