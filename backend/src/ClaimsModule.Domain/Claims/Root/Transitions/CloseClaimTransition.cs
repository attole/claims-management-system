using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Claims.Root.Transitions;

public sealed record CloseClaimInput(
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReserveSnapshot Reserves) : ClaimBaseTransitionInput(ActorRole, ActorId, Now);

public sealed class CloseClaimTransition : ClaimBaseTransition<CloseClaimInput>
{
    public CloseClaimTransition()
        : base(new ClaimClosedState())
    {
        AllowedFrom(new ClaimOpenState(), new ClaimUnderInvestigationState(), new ClaimPendingPaymentState())
            .Require(input => new HasNoPendingReserveChange(input.Reserves))
            .Require(new HasNoUnresolvedCriticalIssues())
            .Require(new HasClaimantForClosure())
            .Require(input => new HasClosingNoteForOpenReserves(input.Reserves, input.Note))
            .Require(input => new HasNoOpenReserveBalance(input.Reserves));
    }
}
