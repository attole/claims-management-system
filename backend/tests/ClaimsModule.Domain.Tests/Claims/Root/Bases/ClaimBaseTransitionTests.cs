using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Events;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Bases;

public sealed class ClaimBaseTransitionTests
{
    [Fact]
    public void Execute_FromADeclaredSource_MovesTheClaimAndTouchesIt()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new SampleClaimTransitionInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later);

        var outcome = new SampleClaimTransition().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.State.Should().BeOfType<ClaimOpenState>();
        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void Execute_RaisesOneStateChangedEventWithTheActorAndBothStates()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        claim.PopEvents();
        var input = new SampleClaimTransitionInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later);

        new SampleClaimTransition().Execute(claim, input);

        var changed = claim.PopEvents().OfType<ClaimStateChanged>().Should().ContainSingle().Subject;
        changed.ClaimId.Should().Be(claim.Id);
        changed.ActorId.Should().Be(TestClaims.Actor);
        changed.From.Should().Be("Draft");
        changed.To.Should().Be("Open");
    }

    [Fact]
    [Trait("Rule", "S:BR-ST-01")]
    [Trait("Rule", TransitionFinding.DefaultCode)]
    public void Execute_FromAnUndeclaredSource_RaisesNoStateChangedEvent()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new SampleClaimTransitionInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later);
        new SampleClaimTransition().Execute(claim, input);
        claim.PopEvents();

        new SampleClaimTransition().Execute(claim, input);

        claim.PopEvents().OfType<ClaimStateChanged>().Should().BeEmpty();
    }

    [Fact]
    public void Execute_WhenARuleBlocks_KeepsTheStateAndRaisesNoStateChangedEvent()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        claim.PopEvents();
        var input = new SampleClaimTransitionInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later);

        var outcome = new BlockedSampleClaimTransition().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(FailingClaimRule.Code);
        claim.State.Should().BeOfType<ClaimDraftState>();
        claim.PopEvents().OfType<ClaimStateChanged>().Should().BeEmpty();
    }

    [Fact]
    public void Execute_ReEvaluatesTheStoredIssues()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var beforeTheLoss = TestClaims.Start.AddDays(-10);
        var input = new SampleClaimTransitionInput(TestClaims.Handler, TestClaims.Actor, beforeTheLoss);

        new SampleClaimTransition().Execute(claim, input);

        claim.State.Should().BeOfType<ClaimOpenState>();
        claim.BlockingIssues.Select(issue => issue.Finding.Code).Should().Equal(IsLossDateNotInFuture.Code);
    }

    [Fact]
    [Trait("Rule", "S:BR-ST-01")]
    [Trait("Rule", TransitionFinding.DefaultCode)]
    public void Execute_FromAnUndeclaredSource_ReturnsTheFindingAndKeepsTheState()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new SampleClaimTransitionInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later);
        new SampleClaimTransition().Execute(claim, input);

        var outcome = new SampleClaimTransition().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    public void Execute_AddsOneStateChangeWithTheActorAndTheReason()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new SampleClaimTransitionInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later)
        {
            Reason = new StateChangeReason("CODE", "text"),
        };

        new SampleClaimTransition().Execute(claim, input);

        var change = claim.StateChanges.Should().ContainSingle().Subject;
        change.From.Should().BeOfType<ClaimDraftState>();
        change.To.Should().BeOfType<ClaimOpenState>();
        change.Reason.Should().Be(new StateChangeReason("CODE", "text"));
        change.CreatedAt.Should().Be(TestClaims.Later);
    }
}
