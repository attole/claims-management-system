using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Common.Roles;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Flows;

internal sealed class SetLimitOverrideOperation
    : Operation<SetLimitOverrideOperation, Claim, SetLimitOverrideInput, Success>
{
    public SetLimitOverrideOperation()
    {
        Require(input => new HasPermittedRole(input.ActorRole, UserRole.Manager));
    }

    protected override Success Apply(Claim claim, SetLimitOverrideInput input)
    {
        claim.HasAggregateLimitOverride = input.HasOverride;

        return Success.Value;
    }
}
