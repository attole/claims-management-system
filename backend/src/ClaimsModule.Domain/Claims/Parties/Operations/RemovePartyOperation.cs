using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Parties.Operations;

public sealed record RemovePartyInput(Guid PartyId, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseOperationInput(Now, ActorId);

public sealed class RemovePartyOperation : Operation<Claim, RemovePartyInput, Success>
{
    public RemovePartyOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new IsPartyActive(input.PartyId));
        Require(input => new IsNotLastClaimant(input.PartyId));
    }

    protected override Success Apply(Claim claim, RemovePartyInput input)
    {
        claim.RemoveParty(input.PartyId, input.ActorId, input.Now);

        return Success.Value;
    }
}
