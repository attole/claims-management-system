using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Parties.Operations;

public sealed record AddPartyInput(PartyDetails Details, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseOperationInput(Now, ActorId);

public sealed class AddPartyOperation : Operation<Claim, AddPartyInput, ClaimParty>
{
    public AddPartyOperation()
    {
        Require(new IsClaimEditable());
    }

    protected override ClaimParty Apply(Claim claim, AddPartyInput input)
    {
        return claim.AddParty(input.Details, input.ActorId, input.Now);
    }
}
