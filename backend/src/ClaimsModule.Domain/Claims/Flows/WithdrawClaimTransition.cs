using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Claims.StateChanges.Rules;

namespace ClaimsModule.Domain.Claims.Flows;

internal sealed class WithdrawClaimTransition : ClaimBaseTransition<WithdrawClaimTransition, FreezeClaimInput>
{
    public WithdrawClaimTransition() : base(new ClaimWithdrawnState())
    {
        AllowedFrom(new ClaimOpenState(), new ClaimUnderInvestigationState())
            .Require(input => new HasStateChangeReason(input.Reason))
            .Require(input => new HasNoPendingReserveChangeForWithdrawal(input.HasPendingReserveChange))
            .Require(input => new HasNoUnpostedReserveChange(input.HasUnpostedReserveChange))
            .Require(input => new HasWithdrawalNoteForOpenReserves(input.HasOpenReserveBalance, input.HasNote))
            .Require(input => new HasNoOpenReserveBalanceOnWithdrawal(input.HasOpenReserveBalance));
    }
}
