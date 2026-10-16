using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.RiskObjects.Flows;

internal sealed class RemoveRiskObjectOperation
    : Operation<RemoveRiskObjectOperation, ClaimRiskObject, RiskObjectRemovalInput, ClaimRiskObject>
{
    public RemoveRiskObjectOperation()
    {
        Require(new IsRiskObjectNotRemoved());
    }

    protected override Outcome<ClaimRiskObject> Apply(ClaimRiskObject riskObject, RiskObjectRemovalInput input)
    {
        riskObject.IsDeleted = true;
        riskObject.DeletedAt = input.Now;

        return Done(riskObject);
    }
}
