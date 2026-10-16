using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.ReserveChanges.Events;

public sealed record ReserveApprovalStateChangedEvent(
    Guid ReserveChangeId,
    Guid ReserveId,
    Guid ClaimId,
    string From,
    string To,
    string Reason) : DomainEvent;
