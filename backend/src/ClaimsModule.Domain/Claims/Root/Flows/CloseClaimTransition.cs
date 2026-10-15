using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;

namespace ClaimsModule.Domain.Claims.Root.Flows;

internal sealed class CloseClaimTransition : ClaimBaseTransition<CloseClaimTransition, CloseClaimInput>
{
    public CloseClaimTransition() : base(new ClaimClosedState())
    {
        AllowedFrom(new ClaimOpenState(), new ClaimUnderInvestigationState(), new ClaimPendingPaymentState())
            .Require(input => new HasNoPendingReserveChange(input.Reserves))
            .Require(new HasNoUnresolvedCriticalIssues())
            .Require(new HasClaimantForClosure())
            .Require(input => new HasClosingNoteForOpenReserves(input.Reserves, input.Note))
            .Require(input => new HasNoOpenReserveBalance(input.Reserves));
    }
}
