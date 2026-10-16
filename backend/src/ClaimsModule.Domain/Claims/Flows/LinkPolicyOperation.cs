using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Flows;

internal sealed class LinkPolicyOperation : Operation<LinkPolicyOperation, Claim, LinkPolicyInput, Claim>
{
    public LinkPolicyOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new HasPolicyId(input.PolicyId));
    }

    protected override Outcome<Claim> Apply(Claim claim, LinkPolicyInput input)
    {
        claim.PolicyId = input.PolicyId;

        return Done(claim);
    }
}
