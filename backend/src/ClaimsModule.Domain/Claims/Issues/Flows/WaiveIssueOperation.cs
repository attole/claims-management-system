using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Issues.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Issues.Flows;

internal sealed class WaiveIssueOperation
    : Operation<WaiveIssueOperation, ClaimValidationIssue, IssueResolutionInput, ClaimValidationIssue>
{
    public WaiveIssueOperation()
    {
        Require(new IsIssueWaivable());
        Require(new HasWaivableFinding());
    }

    protected override Outcome<ClaimValidationIssue> Apply(ClaimValidationIssue issue, IssueResolutionInput input)
    {
        issue.State = new IssueWaivedState();
        issue.ResolvedAt = input.Now;
        issue.ResolvedBy = input.ActorId;

        return Done(issue);
    }
}
