using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.RiskObjects.Flows;

internal sealed class UpdateRiskObjectOnClaimOperation
    : Operation<UpdateRiskObjectOnClaimOperation, Claim, UpdateRiskObjectInput, ClaimRiskObject>
{
    public UpdateRiskObjectOnClaimOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new HasNoOtherPrimaryRiskObject(input.RiskObjectId, input.Details.IsPrimary));
    }

    protected override Outcome<ClaimRiskObject> Apply(Claim claim, UpdateRiskObjectInput input)
    {
        var riskObject = claim.RiskObjects.Single(riskObject => riskObject.Id == input.RiskObjectId);

        return riskObject.Update(new RiskObjectDetailsInput(input.Details));
    }
}
