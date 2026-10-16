using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Parties.Flows;

internal sealed class AddPartyOperation : Operation<AddPartyOperation, Claim, AddPartyInput, ClaimParty>
{
    public AddPartyOperation()
    {
        Require(new IsClaimEditable());
    }

    protected override Outcome<ClaimParty> Apply(Claim claim, AddPartyInput input)
    {
        var party = new ClaimParty(Ids.Next(), claim.Id, claim.OrganisationId, input.Details);
        claim.PartyList.Add(party);

        return Done(party);
    }
}
