using ClaimsModule.Domain.ReserveChanges;
using ClaimsModule.Domain.ReserveChanges.Flows;
using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Flows;

public sealed class ApproveReserveTransitionTests
{
    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    [Trait("Rule", "S:RES-EventSourced")]
    public void Run_FromPendingBySupervisor_ApprovesAndFillsTheBalances()
    {
        var change = TestReserves.Submit(50_000m);
        var input = CreateInput();

        var outcome = ApproveReserveTransition.Run(change, input);

        outcome.Value.To.Should().BeOfType<ReserveApprovedState>();
        change.Balances.PreviousBalance.Should().Be(0m);
        change.Balances.NewBalance.Should().Be(50_000m);
        change.Review.ReviewedBy.Should().Be(TestReserves.Approver);
    }

    [Fact]
    [Trait("Rule", HasApprovalAuthority.Code)]
    public void Run_ByAHandler_ReturnsTheFindingAndKeepsTheApproval()
    {
        var change = TestReserves.Submit(50_000m);
        var input = TestReserves.ApproveInput(TestClaims.Handler, TestReserves.Approver);

        var outcome = ApproveReserveTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasApprovalAuthority.Code);
        change.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", IsNotSelfApproval.Code)]
    public void Run_BySubmitter_ReturnsTheFinding()
    {
        var change = TestReserves.Submit(50_000m);
        var input = TestReserves.ApproveInput(TestClaims.Manager, TestReserves.Submitter);

        var outcome = ApproveReserveTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsNotSelfApproval.Code);
    }

    [Fact]
    [Trait("Rule", IsClaimLimitCleared.Code)]
    public void Run_WhenTheClaimLimitIsPassedWithoutAnOverride_ReturnsTheFinding()
    {
        var change = TestReserves.Submit(8_000m);
        var input = CreateInput() with { ClaimApprovedTotal = 9_995_000m };

        var outcome = ApproveReserveTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimLimitCleared.Code);
    }

    [Fact]
    [Trait("Rule", IsApprovedBalanceNonNegative.Code)]
    public void Run_WhenTheBalanceHasFallenSinceSubmission_ReturnsTheFinding()
    {
        var submit = TestReserves.SubmitInput(-50_000m) with { ReserveApprovedBalance = 100_000m };
        var change = TestReserves.Submit(submit);
        var input = CreateInput() with { ReserveApprovedBalance = 10_000m };

        var outcome = ApproveReserveTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsApprovedBalanceNonNegative.Code);
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingImmutable")]
    public void Run_FromAnAutoApprovedRow_ReturnsNotPermitted()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput();

        var outcome = ApproveReserveTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(ApproveReserveTransition.NotPermittedCode);
    }

    [Fact]
    public void Run_StampedBeforeTheSubmission_Throws()
    {
        var change = TestReserves.Submit(50_000m);
        var earlier = TestClaims.Start.AddHours(-1);
        var input = new ApproveReserveInput(0m, 0m, false, TestClaims.Supervisor, TestReserves.Approver, earlier);

        Action act = () => ApproveReserveTransition.Run(change, input);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static ApproveReserveInput CreateInput()
    {
        return TestReserves.ApproveInput(TestClaims.Supervisor, TestReserves.Approver);
    }
}
