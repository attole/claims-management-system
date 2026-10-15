using ClaimsModule.Domain.Common.Roles;
using ClaimsModule.Domain.ReserveChanges.Root.Bases;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Transitions;

public sealed record ApproveReserveInput(UserRole ActorRole, Guid ActorId, DateTimeOffset Now, ReserveSnapshot Snapshot)
    : ReserveChangeBaseTransitionInput(ActorRole, ActorId, Now, Snapshot);

internal sealed class ApproveReserveTransition : ReserveChangeBaseTransition<ApproveReserveInput>
{
    public ApproveReserveTransition() : base(new ReserveApprovedState())
    {
        AllowedFrom(new ReservePendingApprovalState())
            .Require(input => new HasApprovalAuthority(input.ActorRole))
            .Require(input => new IsNotSelfApproval(input.ActorId))
            .Require(input => new IsClaimLimitCleared(input.Snapshot))
            .Require(input => new IsApprovedBalanceNonNegative(input.Snapshot));
    }
}
