using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.ReserveChanges.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Flows;

internal sealed class AutoApproveReserveTransition
    : ReserveChangeBaseApprovalTransition<AutoApproveReserveTransition, AutoApproveReserveInput>
{
    public AutoApproveReserveTransition() : base(new ReserveAutoApprovedState())
    {
        AllowedFrom(new ReservePendingApprovalState())
            .Require(new IsWithinAutoApprovalLimit())
            .Require(input => new IsClaimLimitCleared(input.ClaimApprovedTotal, hasAggregateLimitOverride: false));
    }
}
