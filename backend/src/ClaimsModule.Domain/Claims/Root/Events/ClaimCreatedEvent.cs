using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Claims.Root.Events;

public sealed record ClaimCreatedEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ActorId,
    Guid ClaimId,
    string ClaimNumber) : DomainEvent(EventId, OccurredAt, ActorId);
