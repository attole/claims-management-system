using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Common;
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
        var riskObject = claim.RiskObjectList.Single(riskObject => riskObject.Id == input.RiskObjectId);

        var deletedAt = InputTime.NotBefore(input.Now, claim.ReportedDate);
        var removal = new RiskObjectRemovalInput(deletedAt);

        return riskObject.Remove(removal);
    }
}
