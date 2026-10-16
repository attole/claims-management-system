using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Issues.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Issues.Flows;

internal sealed class AcknowledgeIssueOperation
    : Operation<AcknowledgeIssueOperation, ClaimValidationIssue, IssueResolutionInput, ClaimValidationIssue>
{
    public AcknowledgeIssueOperation()
    {
        Require(new IsIssueAcknowledgeable());
    }

    protected override Outcome<ClaimValidationIssue> Apply(ClaimValidationIssue issue, IssueResolutionInput input)
    {
        issue.State = new IssueAcknowledgedState();
        issue.ResolvedAt = input.Now;
        issue.ResolvedBy = input.ActorId;

        return Done(issue);
    }
}
