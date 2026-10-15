using ClaimsModule.Domain.ReserveChanges.Root.Bases;
using ClaimsModule.Domain.ReserveChanges.Root.Inputs;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Flows;

internal sealed class AutoApproveReserveTransition
    : ReserveChangeBaseTransition<AutoApproveReserveTransition, AutoApproveReserveInput>
{
    public AutoApproveReserveTransition() : base(new ReserveAutoApprovedState())
    {
        AllowedFrom(new ReservePendingApprovalState())
            .Require(new IsWithinAutoApprovalLimit())
            .Require(input => new IsClaimLimitCleared(input.Snapshot with { HasLimitOverride = false }));
    }
}
