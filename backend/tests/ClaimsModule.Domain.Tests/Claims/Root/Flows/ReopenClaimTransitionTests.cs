using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Flows;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Claims.StateChanges.Rules;
using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Flows;

public sealed class ReopenClaimTransitionTests
{
    private static readonly StateChangeReason _reason = new("NEW_EVIDENCE", "Late evidence arrived");

    [Fact]
    [Trait("Rule", "S:BR-ST-04")]
    [Trait("Rule", "S:ST-Closed-Reopened")]
    public void Execute_ByASupervisorWithAReason_MovesToReopened()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new ReopenClaimInput(
            TestClaims.Supervisor,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference())
        {
            Reason = _reason,
        };

        var outcome = ReopenClaimTransition.Execute(claim, input);

        outcome.Value.To.Should().BeOfType<ClaimReopenedState>();
        claim.State.Should().BeOfType<ClaimReopenedState>();
    }

    [Fact]
    [Trait("Rule", "S:BR-ST-04")]
    public void Execute_ByASupervisorWithAReason_RecordsTheReasonOnTheStateChange()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new ReopenClaimInput(
            TestClaims.Supervisor,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference())
        {
            Reason = _reason,
        };

        ReopenClaimTransition.Execute(claim, input);

        var change = claim.StateChanges.Last();
        change.To.Should().BeOfType<ClaimReopenedState>();
        change.Reason.Should().Be(_reason);
    }

    [Fact]
    [Trait("Rule", HasPermittedRole.Code)]
    public void Execute_ByAHandler_ReturnsTheRoleFinding()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new ReopenClaimInput(TestClaims.Handler, TestClaims.Actor, TestClaims.Later, TestClaims.Reference())
        {
            Reason = _reason,
        };

        var outcome = ReopenClaimTransition.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasPermittedRole.Code);
        claim.State.Should().BeOfType<ClaimClosedState>();
    }

    [Fact]
    [Trait("Rule", HasStateChangeReason.Code)]
    public void Execute_WithoutAReason_ReturnsTheReasonFinding()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new ReopenClaimInput(
            TestClaims.Supervisor,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = ReopenClaimTransition.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasStateChangeReason.Code);
    }

    [Fact]
    [Trait("Rule", HasPermittedRole.Code)]
    [Trait("Rule", HasStateChangeReason.Code)]
    public void Execute_ByAHandlerWithoutAReason_ReturnsBothFindings()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new ReopenClaimInput(
            TestClaims.Handler,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = ReopenClaimTransition.Execute(claim, input);

        outcome.Blocking.Select(finding => finding.Code)
            .Should().BeEquivalentTo(HasPermittedRole.Code, HasStateChangeReason.Code);
    }

    [Fact]
    [Trait("Rule", "S:ST-Other-Invalid")]
    public void Execute_FromOpen_ReturnsNotPermitted()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = new ReopenClaimInput(
            TestClaims.Supervisor,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference())
        {
            Reason = _reason,
        };

        var outcome = ReopenClaimTransition.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
    }
}
