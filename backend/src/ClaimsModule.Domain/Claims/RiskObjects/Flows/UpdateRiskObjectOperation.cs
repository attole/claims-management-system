using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.RiskObjects.Flows;

internal sealed class UpdateRiskObjectOperation
    : Operation<UpdateRiskObjectOperation, Claim, UpdateRiskObjectInput, ClaimRiskObject>
{
    public UpdateRiskObjectOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new IsRiskObjectPresent(input.RiskObjectId));
        Require(input => new HasNoOtherPrimaryRiskObject(input.RiskObjectId, input.Details.IsPrimary));
    }

    protected override ClaimRiskObject Apply(Claim claim, UpdateRiskObjectInput input)
    {
        var riskObject = claim.RiskObjectList
            .Single(candidate => candidate.Id == input.RiskObjectId && !candidate.IsDeleted);
        riskObject.Update(input.Details, input.Now);

        return riskObject;
    }
}
