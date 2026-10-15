using ClaimsModule.Domain.Claims.Issues.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Issues.Operations;

public sealed record AcknowledgeIssueInput(
    Guid IssueId,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseOperationInput(Now, ActorId, Reference);

internal sealed class AcknowledgeIssueOperation : Operation<Claim, AcknowledgeIssueInput, ClaimValidationIssue>
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
