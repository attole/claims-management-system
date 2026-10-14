using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Claims.Root.Events;

public sealed record ClaimStateChanged(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ActorId,
    Guid ClaimId,
    string From,
    string To,
    StateChangeReason Reason) : DomainEvent(EventId, OccurredAt, ActorId);
