using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Events;
using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root;

public sealed class ClaimBaseTransitionTests
{
    [Fact]
    public void Execute_FromADeclaredSource_MovesTheClaimAndTouchesIt()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new SampleClaimTransitionInput(TestClaims.Actor, TestClaims.Later);

        var outcome = new SampleClaimTransition().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.State.Should().BeOfType<ClaimOpenState>();
        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void Execute_RaisesOneStatusChangedEventWithTheActorAndBothStatuses()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        claim.PopEvents();
        var input = new SampleClaimTransitionInput(TestClaims.Actor, TestClaims.Later);

        new SampleClaimTransition().Execute(claim, input);

        var changed = claim.PopEvents().OfType<ClaimStatusChanged>().Should().ContainSingle().Subject;
        changed.ClaimId.Should().Be(claim.Id);
        changed.ActorId.Should().Be(TestClaims.Actor);
        changed.From.Should().Be("Draft");
        changed.To.Should().Be("Open");
    }

    [Fact]
    [Trait("Rule", TransitionFinding.DefaultCode)]
    public void Execute_FromAnUndeclaredSource_RaisesNoStatusChangedEvent()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new SampleClaimTransitionInput(TestClaims.Actor, TestClaims.Later);
        new SampleClaimTransition().Execute(claim, input);
        claim.PopEvents();

        new SampleClaimTransition().Execute(claim, input);

        claim.PopEvents().OfType<ClaimStatusChanged>().Should().BeEmpty();
    }

    [Fact]
    public void Execute_WhenARuleBlocks_KeepsTheStateAndRaisesNoStatusChangedEvent()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        claim.PopEvents();
        var input = new SampleClaimTransitionInput(TestClaims.Actor, TestClaims.Later);

        var outcome = new BlockedSampleClaimTransition().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(FailingClaimRule.Code);
        claim.State.Should().BeOfType<ClaimDraftState>();
        claim.PopEvents().OfType<ClaimStatusChanged>().Should().BeEmpty();
    }

    [Fact]
    public void Execute_ReEvaluatesTheStoredIssues()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var beforeTheLoss = TestClaims.Start.AddDays(-10);
        var input = new SampleClaimTransitionInput(TestClaims.Actor, beforeTheLoss);

        new SampleClaimTransition().Execute(claim, input);

        claim.State.Should().BeOfType<ClaimOpenState>();
        claim.BlockingIssues.Select(issue => issue.Finding.Code).Should().Equal(IsLossDateNotInFuture.Code);
    }

    [Fact]
    [Trait("Rule", TransitionFinding.DefaultCode)]
    public void Execute_FromAnUndeclaredSource_ReturnsTheFindingAndKeepsTheState()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new SampleClaimTransitionInput(TestClaims.Actor, TestClaims.Later);
        new SampleClaimTransition().Execute(claim, input);

        var outcome = new SampleClaimTransition().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
        claim.State.Should().BeOfType<ClaimOpenState>();
    }
}
