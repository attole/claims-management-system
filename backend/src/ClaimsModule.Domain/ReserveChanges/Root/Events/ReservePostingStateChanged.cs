using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.ReserveChanges.Root.Events;

public sealed record ReservePostingStateChanged(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ActorId,
    Guid ReserveChangeId,
    Guid ReserveId,
    Guid ClaimId,
    string From,
    string To,
    string JobId,
    string FailureReason) : DomainEvent(EventId, OccurredAt, ActorId);
