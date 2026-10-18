using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.History.Rules;

namespace ClaimsModule.Domain.ReserveComponents.History.Flows;

internal sealed class AutoApproveHistoryTransition
    : ReserveHistoryBaseApprovalTransition<AutoApproveHistoryTransition, AutoApproveRowInput>
{
    public AutoApproveHistoryTransition() : base(new ReserveAutoApprovedState())
    {
        AllowedFrom(new ReservePendingApprovalState())
            .Require(new IsWithinAutoApprovalLimit())
            .Require(input => new IsClaimLimitCleared(input.ClaimApprovedTotal, hasAggregateLimitOverride: false));
    }
}
