using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Events;
using ClaimsModule.Domain.Claims.Root.Transitions;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Claims.StateChanges.Rules;
using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Transitions;

public sealed class WithdrawClaimTransitionTests
{
    private static readonly StateChangeReason _reason = new("CUSTOMER_REQUEST", "The insured withdrew the claim");

    [Fact]
    [Trait("Rule", "S:ST-Open-Withdrawn")]
    public void Execute_FromOpenWithAReason_WithdrawsTheClaimAndRecordsTheReason()
    {
        var claim = TestClaims.InState(new ClaimOpenState());
        var input = new WithdrawClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later) { Reason = _reason };

        var outcome = new WithdrawClaimTransition().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.State.Should().BeOfType<ClaimWithdrawnState>();
        claim.StateChanges.Last().Reason.Should().Be(_reason);
    }

    [Fact]
    [Trait("Rule", "S:ST-Open-Withdrawn")]
    public void Execute_FromOpenWithAReason_RaisesTheReasonOnTheEvent()
    {
        var claim = TestClaims.InState(new ClaimOpenState());
        var input = new WithdrawClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later) { Reason = _reason };

        new WithdrawClaimTransition().Execute(claim, input);

        var changed = claim.PopEvents().OfType<ClaimStateChanged>().Should().ContainSingle().Subject;
        changed.Reason.Should().Be(_reason);
    }

    [Fact]
    [Trait("Rule", "S:ST-UnderInvestigation-Withdrawn")]
    public void Execute_FromUnderInvestigationWithAReason_WithdrawsTheClaim()
    {
        var claim = TestClaims.InState(new ClaimUnderInvestigationState());
        var input = new WithdrawClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later) { Reason = _reason };

        var outcome = new WithdrawClaimTransition().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", HasStateChangeReason.Code)]
    public void Execute_WithoutAReason_ReturnsTheFindingAndKeepsTheState()
    {
        var claim = TestClaims.InState(new ClaimOpenState());
        var input = new WithdrawClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later);

        var outcome = new WithdrawClaimTransition().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasStateChangeReason.Code);
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    [Trait("Rule", HasStateChangeReason.Code)]
    public void Execute_WithOnlyACode_ReturnsTheFinding()
    {
        var claim = TestClaims.InState(new ClaimOpenState());
        var input = new WithdrawClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later)
        {
            Reason = new StateChangeReason("CUSTOMER_REQUEST", string.Empty),
        };

        var outcome = new WithdrawClaimTransition().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasStateChangeReason.Code);
    }

    [Fact]
    [Trait("Rule", "S:ST-Other-Invalid")]
    public void Execute_FromADraft_ReturnsNotPermitted()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new WithdrawClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later) { Reason = _reason };

        var outcome = new WithdrawClaimTransition().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
    }
}
