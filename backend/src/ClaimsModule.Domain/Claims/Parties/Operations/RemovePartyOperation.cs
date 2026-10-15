using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Parties.Operations;

public sealed record RemovePartyInput(Guid PartyId, Guid ActorId, DateTimeOffset Now, ClaimReferenceSnapshot Reference)
    : ClaimBaseOperationInput(Now, ActorId, Reference);

internal sealed class RemovePartyOperation : Operation<Claim, RemovePartyInput, ClaimParty>
{
    public RemovePartyOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new IsPartyActive(input.PartyId));
        Require(input => new IsNotLastClaimant(input.PartyId));
    }

    protected override ClaimParty Apply(Claim claim, RemovePartyInput input)
    {
        var party = claim.PartyList.Single(candidate => candidate.Id == input.PartyId && candidate.IsActive);
        party.Remove(input.Now);

        return party;
    }
}
