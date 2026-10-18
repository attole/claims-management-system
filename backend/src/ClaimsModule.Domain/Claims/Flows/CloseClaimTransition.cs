using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Rules;

namespace ClaimsModule.Domain.Claims.Flows;

internal sealed class CloseClaimTransition : ClaimBaseTransition<CloseClaimTransition, FreezeClaimInput>
{
    public CloseClaimTransition() : base(new ClaimClosedState())
    {
        AllowedFrom(new ClaimOpenState(), new ClaimUnderInvestigationState(), new ClaimPendingPaymentState())
            .Require(input => new HasNoPendingReserveHistory(input.HasPendingReserveHistory))
            .Require(input => new HasNoUnpostedReserveHistory(input.HasUnpostedReserveHistory))
            .Require(new HasNoCriticalIssuesForClose())
            .Require(new HasClaimantForClosure())
            .Require(input => new HasClosingNoteForOpenReserves(input.HasOpenReserveBalance, input.HasNote))
            .Require(input => new HasNoOpenReserveBalance(input.HasOpenReserveBalance));
    }
}
