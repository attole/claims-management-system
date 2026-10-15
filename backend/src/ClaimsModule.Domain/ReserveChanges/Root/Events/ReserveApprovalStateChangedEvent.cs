using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.ReserveChanges.Root.Events;

public sealed record ReserveApprovalStateChangedEvent(
    EventStamp Stamp,
    Guid ReserveChangeId,
    Guid ReserveId,
    Guid ClaimId,
    string From,
    string To,
    string Reason) : DomainEvent(Stamp);
