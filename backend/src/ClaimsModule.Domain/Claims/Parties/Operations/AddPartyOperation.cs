using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Parties.Operations;

public sealed record AddPartyInput(
    PartyDetails Details,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseOperationInput(Now, ActorId, Reference);

internal sealed class AddPartyOperation : Operation<Claim, AddPartyInput, ClaimParty>
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
