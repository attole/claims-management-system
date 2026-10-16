using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Claims.Events;

public sealed record ClaimStateChangedEvent(
    Guid ClaimId,
    string From,
    string To,
    StateChangeReason Reason) : DomainEvent;
