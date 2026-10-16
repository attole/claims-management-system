using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Claims.Parties.Events;

public sealed record PartyRemovedEvent(
    Guid ClaimId,
    Guid PartyId,
    string Role) : DomainEvent;
