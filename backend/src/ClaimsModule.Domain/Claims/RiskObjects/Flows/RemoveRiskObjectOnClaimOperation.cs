using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.RiskObjects.Flows;

internal sealed class RemoveRiskObjectOnClaimOperation
    : Operation<RemoveRiskObjectOnClaimOperation, Claim, RemoveRiskObjectInput, ClaimRiskObject>
{
    public RemoveRiskObjectOnClaimOperation()
    {
        Require(new IsClaimEditable());
    }

    protected override Outcome<ClaimRiskObject> Apply(Claim claim, RemoveRiskObjectInput input)
    {
        var riskObject = claim.RiskObjects.Single(riskObject => riskObject.Id == input.RiskObjectId);

        return riskObject.Remove();
    }
}
