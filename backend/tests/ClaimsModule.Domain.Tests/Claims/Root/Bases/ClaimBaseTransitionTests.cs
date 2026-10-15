using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Events;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Bases;

public sealed class ClaimBaseTransitionTests
{
    [Fact]
    public void Execute_FromADeclaredSource_MovesTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new SampleClaimTransitionInput(
            TestClaims.Handler,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = SampleClaimTransition.Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    [Trait("Rule", "S:BR-ST-01")]
    [Trait("Rule", TransitionFinding.DefaultCode)]
    public void Execute_FromAnUndeclaredSource_RaisesNoStateChangedEvent()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new SampleClaimTransitionInput(
            TestClaims.Handler,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());
        SampleClaimTransition.Execute(claim, input);
        claim.PopEvents();

        SampleClaimTransition.Execute(claim, input);

        claim.PopEvents().OfType<ClaimStateChangedEvent>().Should().BeEmpty();
    }

    [Fact]
    public void Execute_WhenARuleBlocks_KeepsTheStateAndRaisesNoStateChangedEvent()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        claim.PopEvents();
        var input = new SampleClaimTransitionInput(
            TestClaims.Handler,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = BlockedSampleClaimTransition.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(FailingClaimRule.Code);
        claim.State.Should().BeOfType<ClaimDraftState>();
        claim.PopEvents().OfType<ClaimStateChangedEvent>().Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:BR-ST-01")]
    [Trait("Rule", TransitionFinding.DefaultCode)]
    public void Execute_FromAnUndeclaredSource_ReturnsTheFindingAndKeepsTheState()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new SampleClaimTransitionInput(
            TestClaims.Handler,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());
        SampleClaimTransition.Execute(claim, input);

        var outcome = SampleClaimTransition.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    public void Execute_AddsOneStateChangeWithTheActorAndTheReason()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new SampleClaimTransitionInput(
            TestClaims.Handler,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference())
        {
            Reason = new StateChangeReason("CODE", "text"),
        };

        SampleClaimTransition.Execute(claim, input);

        var change = claim.StateChanges.Should().ContainSingle().Subject;
        change.From.Should().BeOfType<ClaimDraftState>();
        change.To.Should().BeOfType<ClaimOpenState>();
        change.Reason.Should().Be(new StateChangeReason("CODE", "text"));
        change.CreatedAt.Should().Be(TestClaims.Later);
    }
}
