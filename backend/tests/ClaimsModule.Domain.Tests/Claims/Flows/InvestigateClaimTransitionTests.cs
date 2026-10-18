using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Flows;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Flows;

public sealed class InvestigateClaimTransitionTests
{
    [Fact]
    [Trait("Rule", "S:ST-Open-UnderInvestigation")]
    public void Run_FromOpen_MovesTheClaimUnderInvestigation()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = new ClaimTransitionInput(TestClaims.Period(), TestClaims.Codes());

        var outcome = InvestigateClaimTransition.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.State.Should().BeOfType<ClaimUnderInvestigationState>();
    }

    [Fact]
    [Trait("Rule", "S:ST-Other-Invalid")]
    public void Run_FromADraft_ReturnsNotPermitted()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new ClaimTransitionInput(TestClaims.Period(), TestClaims.Codes());

        var outcome = InvestigateClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(InvestigateClaimTransition.NotPermittedCode);
    }
}
