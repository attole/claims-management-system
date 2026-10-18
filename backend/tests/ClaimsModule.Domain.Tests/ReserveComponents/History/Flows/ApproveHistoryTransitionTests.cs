using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Flows;

public sealed class ApproveHistoryTransitionTests
{
    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    [Trait("Rule", "S:RES-EventSourced")]
    public void Run_FromPendingBySupervisor_ApprovesAndFillsTheBalances()
    {
        var history = TestReserves.Submit(50_000m);
        var input = CreateInput();

        var outcome = ApproveHistoryTransition.Run(history, input);

        outcome.Value.To.Should().BeOfType<ReserveApprovedState>();
        history.Balances.PreviousBalance.Should().Be(0m);
        history.Balances.NewBalance.Should().Be(50_000m);
        history.Review.ReviewedBy.Should().Be(TestReserves.Approver);
    }

    [Fact]
    [Trait("Rule", HasApprovalAuthority.Code)]
    public void Run_ByAHandler_ReturnsTheFindingAndKeepsTheApproval()
    {
        var history = TestReserves.Submit(50_000m);
        var input = TestReserves.ApproveInput(TestClaims.Handler, TestReserves.Approver);

        var outcome = ApproveHistoryTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasApprovalAuthority.Code);
        history.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", IsNotSelfApproval.Code)]
    public void Run_BySubmitter_ReturnsTheFinding()
    {
        var history = TestReserves.Submit(50_000m);
        var input = TestReserves.ApproveInput(TestClaims.Manager, TestReserves.Submitter);

        var outcome = ApproveHistoryTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsNotSelfApproval.Code);
    }

    [Fact]
    [Trait("Rule", IsClaimLimitCleared.Code)]
    public void Run_WhenTheClaimLimitIsPassedWithoutAnOverride_ReturnsTheFinding()
    {
        var history = TestReserves.Submit(8_000m);
        var input = CreateInput() with { ClaimApprovedTotal = 9_995_000m };

        var outcome = ApproveHistoryTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimLimitCleared.Code);
    }

    [Fact]
    [Trait("Rule", IsApprovedBalanceNonNegative.Code)]
    public void Run_WhenTheBalanceHasFallenSinceSubmission_ReturnsTheFinding()
    {
        var component = TestReserves.ComponentWithApprovedBalance(100_000m);
        var submit = TestReserves.SubmitInput(-50_000m);
        var history = TestReserves.Submit(component, submit);
        var input = CreateInput() with { ReserveApprovedBalance = 10_000m };

        var outcome = ApproveHistoryTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsApprovedBalanceNonNegative.Code);
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingImmutable")]
    public void Run_FromAnAutoApprovedRow_ReturnsNotPermitted()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput();

        var outcome = ApproveHistoryTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(ApproveHistoryTransition.NotPermittedCode);
    }

    [Fact]
    public void Run_StampedBeforeTheSubmission_Throws()
    {
        var history = TestReserves.Submit(50_000m);
        var earlier = TestClaims.Start.AddHours(-1);
        var input = new ApproveRowInput(0m, 0m, false, TestClaims.Supervisor, TestReserves.Approver, earlier);

        Action act = () => ApproveHistoryTransition.Run(history, input);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static ApproveRowInput CreateInput()
    {
        return TestReserves.ApproveInput(TestClaims.Supervisor, TestReserves.Approver);
    }
}
