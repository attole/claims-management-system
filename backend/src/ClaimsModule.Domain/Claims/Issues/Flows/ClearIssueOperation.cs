using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Issues.Flows;

internal sealed class ClearIssueOperation
    : Operation<ClearIssueOperation, ClaimValidationIssue, NoInput, ClaimValidationIssue>
{
    protected override Outcome<ClaimValidationIssue> Apply(ClaimValidationIssue issue, NoInput input)
    {
        issue.State = new IssueClearedState();

        return Done(issue);
    }
}
