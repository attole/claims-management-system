using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Claims.Root.Transitions;
using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Transitions;

public sealed class SendToPaymentClaimTransitionTests
{
    [Fact]
    [Trait("Rule", "S:ST-Open-PendingPayment")]
    public void Execute_FromOpenWithAnApprovedReserve_MovesToPendingPayment()
    {
        var claim = TestClaims.InState(new ClaimOpenState());
        var input = CreateInput();

        var outcome = new SendToPaymentClaimTransition().Execute(claim, input);

        outcome.Value.Should().BeOfType<ClaimPendingPaymentState>();
        claim.State.Should().BeOfType<ClaimPendingPaymentState>();
    }

    [Fact]
    [Trait("Rule", "S:ST-UnderInvestigation-PendingPayment")]
    public void Execute_FromUnderInvestigationWithLiabilityConfirmed_MovesToPendingPayment()
    {
        var claim = TestClaims.InState(new ClaimUnderInvestigationState());
        var input = CreateInput() with { IsLiabilityConfirmed = true };

        var outcome = new SendToPaymentClaimTransition().Execute(claim, input);

        outcome.Value.Should().BeOfType<ClaimPendingPaymentState>();
    }

    [Fact]
    [Trait("Rule", HasLiabilityConfirmation.Code)]
    public void Execute_FromUnderInvestigationWithoutLiabilityConfirmation_ReturnsTheFinding()
    {
        var claim = TestClaims.InState(new ClaimUnderInvestigationState());
        var input = CreateInput();

        var outcome = new SendToPaymentClaimTransition().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasLiabilityConfirmation.Code);
        claim.State.Should().BeOfType<ClaimUnderInvestigationState>();
    }

    [Fact]
    [Trait("Rule", HasApprovedReserveForOpenClaim.Code)]
    public void Execute_FromOpenWithoutAnApprovedReserve_ReturnsTheFinding()
    {
        var claim = TestClaims.InState(new ClaimOpenState());
        var input = CreateInput() with { Reserves = TestClaims.Reserves() with { ApprovedReserveCount = 0 } };

        var outcome = new SendToPaymentClaimTransition().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasApprovedReserveForOpenClaim.Code);
    }

    [Fact]
    [Trait("Rule", HasApprovedReserveForClaimUnderInvestigation.Code)]
    public void Execute_FromUnderInvestigationWithoutAnApprovedReserve_ReturnsTheFinding()
    {
        var claim = TestClaims.InState(new ClaimUnderInvestigationState());
        var reserves = TestClaims.Reserves() with { ApprovedReserveCount = 0 };
        var input = CreateInput() with { Reserves = reserves, IsLiabilityConfirmed = true };

        var outcome = new SendToPaymentClaimTransition().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code
            .Should().Be(HasApprovedReserveForClaimUnderInvestigation.Code);
    }

    [Fact]
    [Trait("Rule", "S:ST-Other-Invalid")]
    public void Execute_FromADraft_ReturnsNotPermitted()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = CreateInput();

        var outcome = new SendToPaymentClaimTransition().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
    }

    private static SendToPaymentClaimInput CreateInput()
    {
        var reserves = TestClaims.Reserves();

        return new SendToPaymentClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later, reserves);
    }
}
