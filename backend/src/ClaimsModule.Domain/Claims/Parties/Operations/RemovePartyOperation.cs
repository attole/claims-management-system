using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Engine.Operations;

namespace ClaimsModule.Domain.Claims.Parties.Operations;

public sealed record RemovePartyInput(Guid PartyId, Guid ActorId, DateTimeOffset Now) : OperationInput(Now);

public sealed class RemovePartyOperation : Operation<Claim, RemovePartyInput, Success>
{
    public RemovePartyOperation()
    {
        Require(input => new NotLastClaimant(input.PartyId));
    }

    protected override Success Apply(Claim claim, RemovePartyInput input)
    {
        claim.DeactivateParty(input.PartyId, input.ActorId, input.Now);

        return Success.Value;
    }
}
