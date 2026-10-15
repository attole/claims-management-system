using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Operations;

public sealed record LinkPolicyInput(Guid PolicyId, Guid ActorId, DateTimeOffset Now, ClaimReferenceSnapshot Reference)
    : ClaimBaseOperationInput(Now, ActorId, Reference);

internal sealed class LinkPolicyOperation : Operation<Claim, LinkPolicyInput, Success>
{
    public LinkPolicyOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new HasPolicyId(input.PolicyId));
    }

    protected override Success Apply(Claim claim, LinkPolicyInput input)
    {
        claim.PolicyId = input.PolicyId;

        return Success.Value;
    }
}
