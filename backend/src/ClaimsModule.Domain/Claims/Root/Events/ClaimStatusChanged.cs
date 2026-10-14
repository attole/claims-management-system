using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Claims.Root.Events;

public sealed record ClaimStatusChanged(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ActorId,
    Guid ClaimId,
    string From,
    string To) : DomainEvent(EventId, OccurredAt, ActorId);
