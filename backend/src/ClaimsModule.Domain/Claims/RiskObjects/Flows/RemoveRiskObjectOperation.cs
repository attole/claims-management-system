using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.RiskObjects.Flows;

internal sealed class RemoveRiskObjectOperation
    : Operation<RemoveRiskObjectOperation, ClaimRiskObject, NoInput, ClaimRiskObject>
{
    public RemoveRiskObjectOperation()
    {
        Require(new IsRiskObjectNotRemoved());
    }

    protected override Outcome<ClaimRiskObject> Apply(ClaimRiskObject riskObject, NoInput input)
    {
        riskObject.IsDeleted = true;

        return Done(riskObject);
    }
}
