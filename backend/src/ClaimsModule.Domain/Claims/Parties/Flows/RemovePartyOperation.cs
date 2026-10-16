using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Parties.Flows;

internal sealed class RemovePartyOperation : Operation<RemovePartyOperation, ClaimParty, NoInput, ClaimParty>
{
    public RemovePartyOperation()
    {
        Require(new IsPartyActive());
    }

    protected override Outcome<ClaimParty> Apply(ClaimParty party, NoInput input)
    {
        party.IsActive = false;

        return Done(party);
    }
}
