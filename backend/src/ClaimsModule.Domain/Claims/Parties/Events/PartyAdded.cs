using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Claims.Parties.Events;

public sealed record PartyAdded(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ActorId,
    Guid ClaimId,
    Guid PartyId,
    string Role) : DomainEvent(EventId, OccurredAt, ActorId);
