using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Parties.Operations;

public sealed record RemovePartyInput(Guid PartyId, Guid ActorId, DateTimeOffset Now) : ClaimBaseInput(Now, ActorId);

public sealed class RemovePartyOperation : ClaimBaseOperation<RemovePartyInput, Success>
{
    public RemovePartyOperation()
    {
        Require(input => new IsPartyActive(input.PartyId));
        Require(input => new IsNotLastClaimant(input.PartyId));
    }

    protected override Success Apply(Claim claim, RemovePartyInput input)
    {
        claim.DeactivateParty(input.PartyId, input.ActorId, input.Now);

        return Success.Value;
    }
}
