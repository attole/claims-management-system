using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root.Inputs;

namespace ClaimsModule.Domain.Claims.Parties.Inputs;

public sealed record RemovePartyInput(Guid PartyId, Guid ActorId, DateTimeOffset Now, ClaimReferenceSnapshot Reference)
    : ClaimBaseOperationInput(Now, ActorId, Reference);
