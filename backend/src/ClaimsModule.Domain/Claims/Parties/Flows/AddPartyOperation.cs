using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Parties.Flows;

internal sealed class AddPartyOperation : Operation<AddPartyOperation, Claim, AddPartyInput, ClaimParty>
{
    public AddPartyOperation()
    {
        Require(new IsClaimEditable());
    }

    protected override ClaimParty Apply(Claim claim, AddPartyInput input)
    {
        var party = new ClaimParty(
            Ids.Next(input.Now),
            claim.Id,
            claim.OrganisationId,
            input.Details,
            input.Now);
        claim.PartyList.Add(party);

        return party;
    }
}
