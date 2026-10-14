using ClaimsModule.Domain.Claims.Root;

namespace ClaimsModule.Domain.Claims.Parties.Operations;

public sealed record AddPartyInput(PartyDetails Details, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseInput(Now, ActorId);

public sealed class AddPartyOperation : ClaimBaseOperation<AddPartyInput, ClaimParty>
{
    protected override ClaimParty Apply(Claim claim, AddPartyInput input)
    {
        return claim.AddParty(input.Details, input.ActorId, input.Now);
    }
}
