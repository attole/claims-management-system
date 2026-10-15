using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.ReserveChanges.Root.Events;

public sealed record ReserveApprovalStateChanged(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ActorId,
    Guid ReserveChangeId,
    Guid ReserveId,
    Guid ClaimId,
    string From,
    string To,
    string Reason) : DomainEvent(EventId, OccurredAt, ActorId);
