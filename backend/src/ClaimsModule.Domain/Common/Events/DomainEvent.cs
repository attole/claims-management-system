namespace ClaimsModule.Domain.Common.Events;

public abstract record DomainEvent(EventStamp Stamp) : IDomainEvent
{
    public Guid EventId => Stamp.EventId;

    public DateTimeOffset OccurredAt => Stamp.OccurredAt;

    public Guid ActorId => Stamp.ActorId;
}
