using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.RiskObjects.Flows;

internal sealed class AddRiskObjectOperation
    : Operation<AddRiskObjectOperation, Claim, AddRiskObjectInput, ClaimRiskObject>
{
    public AddRiskObjectOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new HasNoOtherPrimaryRiskObject(Guid.Empty, input.Details.IsPrimary));
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
