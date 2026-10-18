using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Roles;
using ClaimsModule.Domain.Roles.Rules;

namespace ClaimsModule.Domain.Claims.Issues.Flows;

internal sealed class WaiveIssueOnClaimOperation
    : Operation<WaiveIssueOnClaimOperation, Claim, WaiveIssueInput, ClaimValidationIssue>
{
    public WaiveIssueOnClaimOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new HasMinimumRole(input.ActorRole, new UserSupervisorRole()));
    }

    protected override Outcome<ClaimValidationIssue> Apply(Claim claim, WaiveIssueInput input)
    {
        var issue = claim.Issues.Single(issue => issue.Id == input.IssueId);

        var resolvedAt = InputTime.NotBefore(input.Now, claim.ReportedDate);
        var resolution = new IssueResolutionInput(input.ActorId, resolvedAt);

        return issue.Waive(resolution);
    }
}
