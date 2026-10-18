using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.History.Rules;

namespace ClaimsModule.Domain.ReserveComponents.History.Flows;

internal sealed class ApproveHistoryTransition
    : ReserveHistoryBaseApprovalTransition<ApproveHistoryTransition, ApproveRowInput>
{
    public ApproveHistoryTransition() : base(new ReserveApprovedState())
    {
        AllowedFrom(new ReservePendingApprovalState())
            .Require(input => new HasApprovalAuthority(input.ActorRole))
            .Require(input => new IsNotSelfApproval(input.ActorId))
            .Require(input => new IsClaimLimitCleared(input.ClaimApprovedTotal, input.HasAggregateLimitOverride))
            .Require(input => new IsApprovedBalanceNonNegative(input.ReserveApprovedBalance));
    }
}
