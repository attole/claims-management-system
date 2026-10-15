using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Claims.Root.Events;

public sealed record ClaimStateChangedEvent(
    EventStamp Stamp,
    Guid ClaimId,
    string From,
    string To,
    StateChangeReason Reason) : DomainEvent(Stamp);
