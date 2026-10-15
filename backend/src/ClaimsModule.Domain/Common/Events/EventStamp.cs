namespace ClaimsModule.Domain.Common.Events;

public sealed record EventStamp(Guid EventId, DateTimeOffset OccurredAt, Guid ActorId);
