using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Parties.Flows;

internal sealed class RemovePartyOperation : Operation<RemovePartyOperation, Claim, RemovePartyInput, ClaimParty>
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
