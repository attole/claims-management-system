using ClaimsModule.Domain.ReserveChanges.Root.Bases;
using ClaimsModule.Domain.ReserveChanges.Root.Inputs;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Flows;

internal sealed class ApproveReserveTransition
    : ReserveChangeBaseTransition<ApproveReserveTransition, ApproveReserveInput>
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
