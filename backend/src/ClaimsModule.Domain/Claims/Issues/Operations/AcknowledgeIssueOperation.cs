using ClaimsModule.Domain.Claims.Issues.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Issues.Operations;

public sealed record AcknowledgeIssueInput(Guid IssueId, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseOperationInput(Now, ActorId);

public sealed class AcknowledgeIssueOperation : Operation<Claim, AcknowledgeIssueInput, Success>
{
    public AcknowledgeIssueOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new IsIssueAcknowledgeable(input.IssueId));
    }

    protected override Success Apply(Claim claim, AcknowledgeIssueInput input)
    {
        claim.AcknowledgeIssue(input.IssueId, input.ActorId, input.Now);

        return Success.Value;
    }
}
