using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.ReserveChanges.Root.Events;

public sealed record ReserveSubmittedEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ActorId,
    Guid ReserveChangeId,
    Guid ReserveId,
    Guid ClaimId,
    decimal Amount) : DomainEvent(EventId, OccurredAt, ActorId);
