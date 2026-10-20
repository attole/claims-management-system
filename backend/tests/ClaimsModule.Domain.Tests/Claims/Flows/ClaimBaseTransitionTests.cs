using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Flows;

public sealed class ClaimBaseTransitionTests
{
    [Fact]
    public void Run_FromADeclaredSource_MovesTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new ClaimTransitionInput(TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);

        var outcome = SampleClaimTransition.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    public void Run_WhenARuleFails_KeepsTheState()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new ClaimTransitionInput(TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);

        var outcome = FailingSampleClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(CriticalSampleClaimRule.Code);
        claim.State.Should().BeOfType<ClaimDraftState>();
    }

    [Fact]
    [Trait("Rule", "S:BR-ST-01")]
    [Trait("Rule", SampleClaimTransition.NotPermittedCode)]
    public void Run_FromAnUndeclaredSource_ReturnsTheFindingAndKeepsTheState()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new ClaimTransitionInput(TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);
        SampleClaimTransition.Run(claim, input);

        var outcome = SampleClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(SampleClaimTransition.NotPermittedCode);
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    public void Run_AddsOneStateChangeWithTheReason()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new ClaimTransitionInput(TestClaims.Period(), TestClaims.Codes(), TestClaims.Start)
        {
            Reason = new StateChangeReason("CODE", "text"),
        };

        SampleClaimTransition.Run(claim, input);

        var change = claim.StateChanges.Should().ContainSingle().Subject;
        change.From.Should().BeOfType<ClaimDraftState>();
        change.To.Should().BeOfType<ClaimOpenState>();
        change.Reason.Should().Be(new StateChangeReason("CODE", "text"));
    }
}
