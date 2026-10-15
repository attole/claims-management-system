using ClaimsModule.Domain.Claims.Issues.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Issues.Operations;

public sealed record WaiveIssueInput(Guid IssueId, Guid ActorId, DateTimeOffset Now, ClaimReferenceSnapshot Reference)
    : ClaimBaseOperationInput(Now, ActorId, Reference);

internal sealed class WaiveIssueOperation : Operation<Claim, WaiveIssueInput, ClaimValidationIssue>
{
    public WaiveIssueOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new IsIssueWaivable(input.IssueId));
    }

    protected override ClaimValidationIssue Apply(Claim claim, WaiveIssueInput input)
    {
        var issue = claim.IssueList.Single(candidate => candidate.Id == input.IssueId);
        issue.Waive(input.ActorId, input.Now);

        return issue;
    }
}
