using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.RiskObjects.Flows;

internal sealed class UpdateRiskObjectOperation
    : Operation<UpdateRiskObjectOperation, ClaimRiskObject, RiskObjectDetailsInput, ClaimRiskObject>
{
    public UpdateRiskObjectOperation()
    {
        Require(new IsRiskObjectNotRemoved());
    }

    protected override Outcome<ClaimRiskObject> Apply(ClaimRiskObject riskObject, RiskObjectDetailsInput input)
    {
        riskObject.Details = input.Details;

        return Done(riskObject);
    }
}
