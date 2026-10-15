using ClaimsModule.Domain.Common.Roles;
using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.ReserveChanges.Root;
using ClaimsModule.Domain.ReserveChanges.Root.Flows;
using ClaimsModule.Domain.ReserveChanges.Root.Inputs;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Flows;

public sealed class RejectReserveTransitionTests
{
    private const string Reason = "Not supported by the report";

    [Fact]
    [Trait("Rule", "S:BR-R-04")]
    public void Execute_BySupervisorWithAReason_RejectsAndRecordsTheReview()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = CreateInput(TestClaims.Supervisor, Reason);

        var outcome = RejectReserveTransition.Execute(reserve, input);

        outcome.Value.To.Should().BeOfType<ReserveRejectedState>();
        reserve.Review.ReviewedBy.Should().Be(TestReserves.Approver);
        reserve.Review.Reason.Should().Be(Reason);
        reserve.Balances.IsSet.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", HasApprovalRole.Code)]
    public void Execute_ByAHandler_ReturnsTheFinding()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = CreateInput(TestClaims.Handler, Reason);

        var outcome = RejectReserveTransition.Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasApprovalRole.Code);
        reserve.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", HasRejectionReason.Code)]
    public void Execute_WithoutAReason_ReturnsTheFinding()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = CreateInput(TestClaims.Supervisor, string.Empty);

        var outcome = RejectReserveTransition.Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasRejectionReason.Code);
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingImmutable")]
    public void Execute_FromAnAutoApprovedRow_ReturnsNotPermitted()
    {
        var reserve = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput(TestClaims.Supervisor, Reason);

        var outcome = RejectReserveTransition.Execute(reserve, input);

        outcome.Blocking.Should().ContainSingle().Which.Should().BeOfType<TransitionFinding>();
    }

    private static RejectReserveInput CreateInput(UserRole role, string reason)
    {
        return new RejectReserveInput(role, TestReserves.Approver, TestClaims.Later, TestReserves.Snapshot())
        {
            Reason = reason,
        };
    }
}
