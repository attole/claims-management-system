using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Issues.Flows;

internal sealed class AcknowledgeIssueOnClaimOperation
    : Operation<AcknowledgeIssueOnClaimOperation, Claim, AcknowledgeIssueInput, ClaimValidationIssue>
{
    public AcknowledgeIssueOnClaimOperation()
    {
        Require(new IsClaimEditable());
    }

    protected override Outcome<ClaimValidationIssue> Apply(Claim claim, AcknowledgeIssueInput input)
    {
        var issue = claim.IssueList.Single(issue => issue.Id == input.IssueId);

        var resolvedAt = InputTime.NotBefore(input.Now, claim.ReportedDate);
        var resolution = new IssueResolutionInput(input.ActorId, resolvedAt);

        return issue.Acknowledge(resolution);
    }
}
