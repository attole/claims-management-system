using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Engine.Operations;

namespace ClaimsModule.Domain.Claims.Parties.Operations;

public sealed record AddPartyInput(PartyDetails Details, Guid ActorId, DateTimeOffset Now) : OperationInput(Now);

public sealed class AddPartyOperation : Operation<Claim, AddPartyInput, ClaimParty>
{
    protected override ClaimParty Apply(Claim claim, AddPartyInput input)
    {
        return claim.AddParty(input.Details, input.ActorId, input.Now);
    }
}
