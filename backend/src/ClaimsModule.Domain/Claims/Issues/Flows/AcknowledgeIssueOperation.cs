using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Issues.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Issues.Flows;

internal sealed class AcknowledgeIssueOperation
    : Operation<AcknowledgeIssueOperation, Claim, AcknowledgeIssueInput, ClaimValidationIssue>
{
    public AcknowledgeIssueOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new IsIssueAcknowledgeable(input.IssueId));
    }

    protected override ClaimValidationIssue Apply(Claim claim, AcknowledgeIssueInput input)
    {
        var issue = claim.IssueList.Single(candidate => candidate.Id == input.IssueId);
        issue.Acknowledge(input.ActorId, input.Now);

        return issue;
    }
}
