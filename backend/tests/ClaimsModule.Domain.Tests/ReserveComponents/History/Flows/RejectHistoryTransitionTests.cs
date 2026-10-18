using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Roles;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Flows;

public sealed class RejectHistoryTransitionTests
{
    private const string Reason = "Not supported by the report";

    [Fact]
    [Trait("Rule", "S:BR-R-04")]
    [Trait("Rule", "S:API-POST-reserve-reject")]
    [Trait("Rule", "A:BR-R-06")]
    public void Run_BySupervisorWithAReason_RejectsAndRecordsTheReview()
    {
        var history = TestReserves.Submit(50_000m);
        var input = CreateInput(TestClaims.Supervisor, Reason);

        var outcome = RejectHistoryTransition.Run(history, input);

        outcome.Value.To.Should().BeOfType<ReserveRejectedState>();
        history.Review.ReviewedBy.Should().Be(TestReserves.Approver);
        history.Review.Reason.Should().Be(Reason);
        history.Balances.IsSet.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", HasRejectionAuthority.Code)]
    public void Run_ByAHandler_ReturnsTheFinding()
    {
        var history = TestReserves.Submit(50_000m);
        var input = CreateInput(TestClaims.Handler, Reason);

        var outcome = RejectHistoryTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasRejectionAuthority.Code);
        history.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", HasRejectionReason.Code)]
    public void Run_WithoutAReason_ReturnsTheFinding()
    {
        var history = TestReserves.Submit(50_000m);
        var input = CreateInput(TestClaims.Supervisor, string.Empty);

        var outcome = RejectHistoryTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasRejectionReason.Code);
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingImmutable")]
    public void Run_FromAnAutoApprovedRow_ReturnsNotPermitted()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput(TestClaims.Supervisor, Reason);

        var outcome = RejectHistoryTransition.Run(history, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(RejectHistoryTransition.NotPermittedCode);
    }

    private static RejectRowInput CreateInput(UserRole role, string reason)
    {
        return new RejectRowInput(role, TestReserves.Approver, TestClaims.Later)
        {
            Reason = reason,
        };
    }
}
