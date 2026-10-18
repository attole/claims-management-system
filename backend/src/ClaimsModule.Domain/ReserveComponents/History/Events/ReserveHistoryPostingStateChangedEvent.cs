using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.ReserveComponents.History.Events;

public sealed record ReserveHistoryPostingStateChangedEvent(
    Guid ReserveHistoryId,
    Guid ReserveComponentId,
    Guid ClaimId,
    string From,
    string To,
    string JobId,
    string FailureReason) : DomainEvent;
