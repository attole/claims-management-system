namespace ClaimsModule.Domain.Common.Events;

public interface IDomainEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAt { get; }

    Guid ActorId { get; }
}
