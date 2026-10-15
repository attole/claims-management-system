using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Claims.Root.Transitions;

public sealed record SendToPaymentClaimInput(
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReserveSnapshot Reserves) : ClaimBaseTransitionInput(ActorRole, ActorId, Now)
{
    public bool IsLiabilityConfirmed { get; init; } = false;
}

public sealed class SendToPaymentClaimTransition : ClaimBaseTransition<SendToPaymentClaimInput>
{
    public SendToPaymentClaimTransition()
        : base(new ClaimPendingPaymentState())
    {
        AllowedFrom(new ClaimOpenState())
            .Require(input => new HasApprovedReserveForOpenClaim(input.Reserves));
        AllowedFrom(new ClaimUnderInvestigationState())
            .Require(input => new HasApprovedReserveForClaimUnderInvestigation(input.Reserves))
            .Require(input => new HasLiabilityConfirmation(input.IsLiabilityConfirmed));
    }
}
