using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Operations;

public sealed record CreateClaimInput(Guid OrganisationId, ClaimNumber ClaimNumber, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseOperationInput(Now, ActorId);

public sealed class CreateClaimOperation : CreationOperation<CreateClaimInput, Claim>
{
    private readonly EvaluateIssuesFlow _issues = new();

    public CreateClaimOperation()
    {
        Require(input => new HasOrganisation(input.OrganisationId));
        Require(input => new HasClaimNumber(input.ClaimNumber));
    }

    protected override Claim Apply(CreateClaimInput input)
    {
        var claim = Claim.Create(input.OrganisationId, input.ClaimNumber, input.ActorId, input.Now);
        _issues.Execute(claim, new EvaluateIssuesInput(input.Now, input.ActorId));

        return claim;
    }
}
