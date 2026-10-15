using ClaimsModule.Domain.Common.Roles;
using ClaimsModule.Domain.ReserveChanges.Root.Bases;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Transitions;

public sealed record AutoApproveReserveInput(
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now,
    ReserveSnapshot Snapshot) : ReserveChangeBaseTransitionInput(ActorRole, ActorId, Now, Snapshot);

public sealed class AutoApproveReserveTransition : ReserveChangeBaseTransition<AutoApproveReserveInput>
{
    public AutoApproveReserveTransition()
        : base(new ReserveAutoApprovedState())
    {
        AllowedFrom(new ReservePendingApprovalState())
            .Require(new IsWithinAutoApprovalLimit())
            .Require(input => new IsClaimLimitCleared(input.Snapshot with { HasLimitOverride = false }));
    }
}
