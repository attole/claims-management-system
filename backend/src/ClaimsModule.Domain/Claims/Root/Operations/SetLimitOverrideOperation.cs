using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Common.Roles;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Operations;

public sealed record SetLimitOverrideInput(
    bool HasOverride,
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseOperationInput(Now, ActorId, Reference);

internal sealed class SetLimitOverrideOperation : Operation<Claim, SetLimitOverrideInput, Success>
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
