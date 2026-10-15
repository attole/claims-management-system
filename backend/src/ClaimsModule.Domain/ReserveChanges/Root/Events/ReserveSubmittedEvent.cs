using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.ReserveChanges.Root.Events;

public sealed record ReserveSubmittedEvent(
    EventStamp Stamp,
    Guid ReserveChangeId,
    Guid ReserveId,
    Guid ClaimId,
    decimal Amount) : DomainEvent(Stamp);
