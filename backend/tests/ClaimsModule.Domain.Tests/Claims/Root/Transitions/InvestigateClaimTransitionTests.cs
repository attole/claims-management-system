using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Claims.Root.Transitions;
using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Transitions;

public sealed class InvestigateClaimTransitionTests
{
    [Fact]
    [Trait("Rule", "S:ST-Open-UnderInvestigation")]
    public void Execute_ByAHandler_MovesAnOpenClaimUnderInvestigation()
    {
        var claim = TestClaims.InState(new ClaimOpenState());
        var input = new InvestigateClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later);

        var outcome = new InvestigateClaimTransition().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.State.Should().BeOfType<ClaimUnderInvestigationState>();
    }

    [Fact]
    [Trait("Rule", "S:ST-Open-UnderInvestigation")]
    public void Execute_ByASupervisor_MovesAnOpenClaimUnderInvestigation()
    {
        var claim = TestClaims.InState(new ClaimOpenState());
        var input = new InvestigateClaimInput(TestClaims.Supervisor, TestClaims.Actor, TestClaims.Later);

        var outcome = new InvestigateClaimTransition().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", HasPermittedRole.Code)]
    public void Execute_ByAManager_ReturnsTheRoleFinding()
    {
        var claim = TestClaims.InState(new ClaimOpenState());
        var input = new InvestigateClaimInput(TestClaims.Manager, TestClaims.Actor, TestClaims.Later);

        var outcome = new InvestigateClaimTransition().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasPermittedRole.Code);
    }

    [Fact]
    [Trait("Rule", "S:ST-Other-Invalid")]
    public void Execute_FromADraft_ReturnsNotPermitted()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new InvestigateClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later);

        var outcome = new InvestigateClaimTransition().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
    }
}
