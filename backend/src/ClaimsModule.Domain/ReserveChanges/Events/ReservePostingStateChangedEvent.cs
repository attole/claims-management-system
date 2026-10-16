using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.ReserveChanges.Events;

public sealed record ReservePostingStateChangedEvent(
    Guid ReserveChangeId,
    Guid ReserveId,
    Guid ClaimId,
    string From,
    string To,
    string JobId,
    string FailureReason) : DomainEvent;
