using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Parties.Flows;

internal sealed class RemovePartyOnClaimOperation
    : Operation<RemovePartyOnClaimOperation, Claim, RemovePartyInput, ClaimParty>
{
    public RemovePartyOnClaimOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new IsNotLastClaimant(input.PartyId));
    }

    protected override Outcome<ClaimParty> Apply(Claim claim, RemovePartyInput input)
    {
        var party = claim.Parties.Single(party => party.Id == input.PartyId);

        return party.Remove();
    }
}
