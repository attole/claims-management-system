using ClaimsModule.Domain.Claims.Issues.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Issues.Operations;

public sealed record WaiveIssueInput(Guid IssueId, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseOperationInput(Now, ActorId);

public sealed class WaiveIssueOperation : Operation<Claim, WaiveIssueInput, Success>
{
    public WaiveIssueOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new IsIssueWaivable(input.IssueId));
    }

    protected override Success Apply(Claim claim, WaiveIssueInput input)
    {
        claim.WaiveIssue(input.IssueId, input.ActorId, input.Now);

        return Success.Value;
    }
}
