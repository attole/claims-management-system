using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Flows;

internal sealed class CreateClaimOperation : CreationOperation<CreateClaimOperation, CreateClaimInput, Claim>
{
    public CreateClaimOperation()
    {
        Require(input => new HasOrganisation(input.OrganisationId));
        Require(input => new HasClaimNumber(input.ClaimNumber));
    }

    protected override Claim Apply(CreateClaimInput input)
    {
        return new Claim(Ids.Next(input.Now), input.OrganisationId, input.ClaimNumber, input.Now);
    }
}
