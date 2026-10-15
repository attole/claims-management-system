using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.RiskObjects.Operations;

public sealed record AddRiskObjectInput(
    RiskObjectDetails Details,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseOperationInput(Now, ActorId, Reference);

internal sealed class AddRiskObjectOperation : Operation<Claim, AddRiskObjectInput, ClaimRiskObject>
{
    public AddRiskObjectOperation()
    {
        Require(new IsClaimEditable());
    }

    protected override ClaimRiskObject Apply(Claim claim, AddRiskObjectInput input)
    {
        var riskObject = new ClaimRiskObject(
            Ids.Next(input.Now),
            claim.Id,
            claim.OrganisationId,
            input.Details,
            input.Now);
        claim.RiskObjectList.Add(riskObject);

        return riskObject;
    }
}
