using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Claims.Root.Transitions;
using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Transitions;

public sealed class OpenClaimTransitionTests
{
    [Fact]
    [Trait("Rule", "S:ST-Draft-Open")]
    public void Execute_FromDraftWhenTheClaimIsComplete_OpensIt()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new OpenClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later);

        var outcome = new OpenClaimTransition().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    [Trait("Rule", HasNoBlockingIssues.Code)]
    [Trait("Rule", HasActiveClaimant.Code)]
    public void Execute_FromDraftWithUnresolvedCriticalIssues_ReturnsBothFindings()
    {
        var claim = TestClaims.Empty();
        var input = new OpenClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later);

        var outcome = new OpenClaimTransition().Execute(claim, input);

        outcome.Blocking.Select(finding => finding.Code)
            .Should().BeEquivalentTo(HasNoBlockingIssues.Code, HasActiveClaimant.Code);
        claim.State.Should().BeOfType<ClaimDraftState>();
    }

    [Fact]
    [Trait("Rule", HasActiveClaimant.Code)]
    public void Execute_FromDraftWithoutAClaimantEvenWhenItsIssueIsWaived_ReturnsTheClaimantFinding()
    {
        var claim = TestClaims.Create(TestClaims.Witness());
        TestClaims.WaiveIssue(claim, HasClaimant.Code);
        var input = new OpenClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later);

        var outcome = new OpenClaimTransition().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasActiveClaimant.Code);
    }

    [Fact]
    [Trait("Rule", "S:ST-Draft-Open")]
    public void Execute_FromDraftWhenTheCriticalIssueIsWaived_OpensIt()
    {
        var claim = TestClaims.WithLoss(TestClaims.Start.AddDays(2));
        TestClaims.WaiveIssue(claim, IsLossDateNotInFuture.Code);
        var input = new OpenClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later);

        var outcome = new OpenClaimTransition().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", "S:ST-UnderInvestigation-Open")]
    public void Execute_FromUnderInvestigationByAHandler_OpensIt()
    {
        var claim = TestClaims.InState(new ClaimUnderInvestigationState());
        var input = new OpenClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later);

        var outcome = new OpenClaimTransition().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    [Trait("Rule", HasPermittedRole.Code)]
    public void Execute_FromUnderInvestigationByASupervisor_ReturnsTheRoleFinding()
    {
        var claim = TestClaims.InState(new ClaimUnderInvestigationState());
        var input = new OpenClaimInput(TestClaims.Supervisor, TestClaims.Actor, TestClaims.Later);

        var outcome = new OpenClaimTransition().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasPermittedRole.Code);
        claim.State.Should().BeOfType<ClaimUnderInvestigationState>();
    }

    [Fact]
    [Trait("Rule", "S:ST-Other-Invalid")]
    public void Execute_FromAnUndeclaredSource_ReturnsNotPermitted()
    {
        var claim = TestClaims.InState(new ClaimOpenState());
        var input = new OpenClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later);

        var outcome = new OpenClaimTransition().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
    }
}
