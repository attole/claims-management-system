using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Common.Entities;

public abstract class Entity : IEquatable<Entity>, ITenantOwned
{
    private readonly List<DomainEvent> _events = [];

    protected Entity()
    {
    }

    protected Entity(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; protected set; } = Guid.Empty;

    public Guid OrganisationId { get; protected set; } = Guid.Empty;

    public IReadOnlyList<DomainEvent> PopEvents()
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

    protected void Raise(DomainEvent domainEvent)
    {
        _events.Add(domainEvent);
    }
}
