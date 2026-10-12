using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Claims.Parties.Events;

public sealed record PartyRemoved(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ActorId,
    Guid ClaimId,
    Guid PartyId,
    string Role) : DomainEvent(EventId, OccurredAt, ActorId);
