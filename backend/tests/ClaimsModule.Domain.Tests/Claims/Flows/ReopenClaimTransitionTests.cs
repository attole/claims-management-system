using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Flows;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Claims.StateChanges.Rules;
using ClaimsModule.Domain.Roles.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Flows;

public sealed class ReopenClaimTransitionTests
{
    private static readonly StateChangeReason _reason = new("NEW_EVIDENCE", "Late evidence arrived");

    [Fact]
    [Trait("Rule", "S:BR-ST-04")]
    [Trait("Rule", "S:ST-Closed-Reopened")]
    public void Run_ByASupervisorWithAReason_MovesToReopened()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new ReopenClaimInput(TestClaims.Supervisor, TestClaims.Period(), TestClaims.Codes())
        {
            Reason = _reason,
        };

        var outcome = ReopenClaimTransition.Run(claim, input);

        outcome.Value.To.Should().BeOfType<ClaimReopenedState>();
        claim.State.Should().BeOfType<ClaimReopenedState>();
    }

    [Fact]
    [Trait("Rule", HasMinimumRole.Code)]
    public void Run_ByAHandler_ReturnsTheRoleFinding()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new ReopenClaimInput(TestClaims.Handler, TestClaims.Period(), TestClaims.Codes())
        {
            Reason = _reason,
        };

        var outcome = ReopenClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasMinimumRole.Code);
        claim.State.Should().BeOfType<ClaimClosedState>();
    }

    [Fact]
    [Trait("Rule", HasStateChangeReason.Code)]
    public void Run_WithoutAReason_ReturnsTheReasonFinding()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new ReopenClaimInput(TestClaims.Supervisor, TestClaims.Period(), TestClaims.Codes());

        var outcome = ReopenClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasStateChangeReason.Code);
    }

    [Fact]
    [Trait("Rule", "S:ST-Other-Invalid")]
    public void Run_FromOpen_ReturnsNotPermitted()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = new ReopenClaimInput(TestClaims.Supervisor, TestClaims.Period(), TestClaims.Codes())
        {
            Reason = _reason,
        };

        var outcome = ReopenClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(ReopenClaimTransition.NotPermittedCode);
    }
}
