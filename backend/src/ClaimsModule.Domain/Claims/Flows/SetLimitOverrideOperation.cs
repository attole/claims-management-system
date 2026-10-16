using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Roles;
using ClaimsModule.Domain.Roles.Rules;

namespace ClaimsModule.Domain.Claims.Flows;

internal sealed class SetLimitOverrideOperation
    : Operation<SetLimitOverrideOperation, Claim, SetLimitOverrideInput, Claim>
{
    public SetLimitOverrideOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new HasMinimumRole(input.ActorRole, new UserManagerRole()));
        Require(input => new IsOverrideRevocable(input.HasAggregateLimitOverride, input.ClaimApprovedTotal));
    }

    protected override Outcome<Claim> Apply(Claim claim, SetLimitOverrideInput input)
    {
        claim.HasAggregateLimitOverride = input.HasAggregateLimitOverride;

        return Done(claim);
    }
}
