using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Common.Entities;

public abstract class Entity : IEquatable<Entity>
{
    private readonly List<IDomainEvent> _events = [];

    protected Entity()
    {
    }

    protected Entity(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; protected set; } = Guid.Empty;

    public IReadOnlyList<IDomainEvent> PopEvents()
    {
        var events = _events.ToList();
        _events.Clear();

        return events;
    }

    public bool Equals(Entity? other)
    {
        return other is not null && GetType() == other.GetType() && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Entity);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    protected void Raise(IDomainEvent domainEvent)
    {
        _events.Add(domainEvent);
    }
}
