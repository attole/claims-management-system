using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.ReserveChanges.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Flows;

internal sealed class ApproveReserveTransition
    : ReserveChangeBaseApprovalTransition<ApproveReserveTransition, ApproveReserveInput>
{
    public ApproveReserveTransition() : base(new ReserveApprovedState())
    {
        AllowedFrom(new ReservePendingApprovalState())
            .Require(input => new HasApprovalAuthority(input.ActorRole))
            .Require(input => new IsNotSelfApproval(input.ActorId))
            .Require(input => new IsClaimLimitCleared(input.ClaimApprovedTotal, input.HasAggregateLimitOverride))
            .Require(input => new IsApprovedBalanceNonNegative(input.ReserveApprovedBalance));
    }
}
