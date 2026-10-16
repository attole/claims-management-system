using ClaimsModule.Domain.ReserveChanges;
using ClaimsModule.Domain.ReserveChanges.Flows;
using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Roles;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Flows;

public sealed class RejectReserveTransitionTests
{
    private const string Reason = "Not supported by the report";

    [Fact]
    [Trait("Rule", "S:BR-R-04")]
    [Trait("Rule", "S:API-POST-reserve-reject")]
    [Trait("Rule", "A:BR-R-06")]
    public void Run_BySupervisorWithAReason_RejectsAndRecordsTheReview()
    {
        var change = TestReserves.Submit(50_000m);
        var input = CreateInput(TestClaims.Supervisor, Reason);

        var outcome = RejectReserveTransition.Run(change, input);

        outcome.Value.To.Should().BeOfType<ReserveRejectedState>();
        change.Review.ReviewedBy.Should().Be(TestReserves.Approver);
        change.Review.Reason.Should().Be(Reason);
        change.Balances.IsSet.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", HasRejectionAuthority.Code)]
    public void Run_ByAHandler_ReturnsTheFinding()
    {
        var change = TestReserves.Submit(50_000m);
        var input = CreateInput(TestClaims.Handler, Reason);

        var outcome = RejectReserveTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasRejectionAuthority.Code);
        change.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", HasRejectionReason.Code)]
    public void Run_WithoutAReason_ReturnsTheFinding()
    {
        var change = TestReserves.Submit(50_000m);
        var input = CreateInput(TestClaims.Supervisor, string.Empty);

        var outcome = RejectReserveTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasRejectionReason.Code);
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingImmutable")]
    public void Run_FromAnAutoApprovedRow_ReturnsNotPermitted()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var input = CreateInput(TestClaims.Supervisor, Reason);

        var outcome = RejectReserveTransition.Run(change, input);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(RejectReserveTransition.NotPermittedCode);
    }

    private static RejectReserveInput CreateInput(UserRole role, string reason)
    {
        return new RejectReserveInput(role, TestReserves.Approver, TestClaims.Later)
        {
            Reason = reason,
        };
    }
}
