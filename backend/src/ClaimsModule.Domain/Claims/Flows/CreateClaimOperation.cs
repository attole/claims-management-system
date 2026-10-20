using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Flows;

internal sealed class CreateClaimOperation : CreationOperation<CreateClaimOperation, CreateClaimInput, Claim>
{
    public CreateClaimOperation()
    {
        Require(input => new HasOrganisation(input.OrganisationId));
        Require(input => new HasClaimNumber(input.ClaimNumber));
        Require(input => new HasReportDate(input.ReportDate));
    }

    protected override Outcome<Claim> Apply(CreateClaimInput input)
    {
        var claim = new Claim(Ids.Next(), input.OrganisationId, input.ClaimNumber, input.ReportDate, input.Now);

        return Done(claim);
    }
}
