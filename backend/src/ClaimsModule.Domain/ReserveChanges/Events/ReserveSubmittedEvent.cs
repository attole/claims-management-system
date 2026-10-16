using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.ReserveChanges.Events;

public sealed record ReserveSubmittedEvent(
    Guid ReserveChangeId,
    Guid ReserveId,
    Guid ClaimId,
    decimal Amount) : DomainEvent;
