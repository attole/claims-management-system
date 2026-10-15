using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.RiskObjects.Flows;

internal sealed class RemoveRiskObjectOperation
    : Operation<RemoveRiskObjectOperation, Claim, RemoveRiskObjectInput, ClaimRiskObject>
{
    public RemoveRiskObjectOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new IsRiskObjectPresent(input.RiskObjectId));
    }

    protected override ClaimRiskObject Apply(Claim claim, RemoveRiskObjectInput input)
    {
        var riskObject = claim.RiskObjectList
            .Single(candidate => candidate.Id == input.RiskObjectId && !candidate.IsDeleted);
        riskObject.Remove(input.Now);

        return riskObject;
    }
}
