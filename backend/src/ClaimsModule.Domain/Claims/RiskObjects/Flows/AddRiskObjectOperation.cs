using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.RiskObjects.Flows;

internal sealed class AddRiskObjectOperation
    : Operation<AddRiskObjectOperation, Claim, AddRiskObjectInput, ClaimRiskObject>
{
    public AddRiskObjectOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new HasNoOtherPrimaryRiskObject(Guid.Empty, input.Details.IsPrimary));
    }

    protected override Outcome<ClaimRiskObject> Apply(Claim claim, AddRiskObjectInput input)
    {
        var riskObject = new ClaimRiskObject(Ids.Next(), claim.Id, claim.OrganisationId, input.Details);
        claim.AttachRiskObject(riskObject);

        return Done(riskObject);
    }
}
