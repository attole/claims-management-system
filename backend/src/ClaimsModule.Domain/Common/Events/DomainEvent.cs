namespace ClaimsModule.Domain.Common.Events;

public abstract record DomainEvent(Guid EventId, DateTimeOffset OccurredAt, Guid ActorId) : IDomainEvent;
