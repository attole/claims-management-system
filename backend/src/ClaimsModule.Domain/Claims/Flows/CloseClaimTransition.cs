using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Rules;

namespace ClaimsModule.Domain.Claims.Flows;

internal sealed class CloseClaimTransition : ClaimBaseTransition<CloseClaimTransition, FreezeClaimInput>
{
    public CloseClaimTransition() : base(new ClaimClosedState())
    {
        AllowedFrom(new ClaimOpenState(), new ClaimUnderInvestigationState(), new ClaimPendingPaymentState())
            .Require(input => new HasNoPendingReserveChange(input.HasPendingReserveChange))
            .Require(input => new HasNoUnpostedReserveChange(input.HasUnpostedReserveChange))
            .Require(new HasNoCriticalIssuesForClose())
            .Require(new HasClaimantForClosure())
            .Require(input => new HasClosingNoteForOpenReserves(input.HasOpenReserveBalance, input.HasNote))
            .Require(input => new HasNoOpenReserveBalance(input.HasOpenReserveBalance));
    }
}
