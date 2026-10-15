using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.ReserveChanges.Root.Events;

public sealed record ReservePostingStateChangedEvent(
    EventStamp Stamp,
    Guid ReserveChangeId,
    Guid ReserveId,
    Guid ClaimId,
    string From,
    string To,
    string JobId,
    string FailureReason) : DomainEvent(Stamp);
