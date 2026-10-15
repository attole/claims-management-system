using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Flows;

internal sealed class LinkPolicyOperation : Operation<LinkPolicyOperation, Claim, LinkPolicyInput, Success>
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
