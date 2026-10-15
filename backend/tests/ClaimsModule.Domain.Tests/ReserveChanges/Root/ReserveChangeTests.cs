using ClaimsModule.Domain.ReserveChanges.Root;
using ClaimsModule.Domain.ReserveChanges.Root.Events;
using ClaimsModule.Domain.ReserveChanges.Root.Flows;
using ClaimsModule.Domain.ReserveChanges.Root.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root;

public sealed class ReserveChangeTests
{
    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void IsApproved_ForAnAutoApprovedRow_IsTrue()
    {
        var reserve = TestReserves.SubmitAutoApproved(5_000m);

        reserve.IsApproved.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void IsApproved_ForAPendingRow_IsFalse()
    {
        var reserve = TestReserves.Submit(50_000m);

        reserve.IsApproved.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void IsApproved_AfterAnApproval_IsTrue()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = new ApproveReserveInput(
            TestClaims.Supervisor,
            TestReserves.Approver,
            TestClaims.Later,
            TestReserves.Snapshot());
        ApproveReserveTransition.Execute(reserve, input);

        reserve.IsApproved.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void IsRejected_ForAPendingRow_IsFalse()
    {
        var reserve = TestReserves.Submit(50_000m);

        reserve.IsRejected.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void IsRejected_AfterARejection_IsTrue()
    {
        var reserve = TestReserves.Submit(50_000m);
        var snapshot = TestReserves.Snapshot();
        var input = new RejectReserveInput(TestClaims.Supervisor, TestReserves.Approver, TestClaims.Later, snapshot)
        {
            Reason = "Not supported by the report",
        };
        RejectReserveTransition.Execute(reserve, input);

        reserve.IsRejected.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void IsApproved_AfterARejection_IsFalse()
    {
        var reserve = TestReserves.Submit(50_000m);
        var snapshot = TestReserves.Snapshot();
        var input = new RejectReserveInput(TestClaims.Supervisor, TestReserves.Approver, TestClaims.Later, snapshot)
        {
            Reason = "Not supported by the report",
        };
        RejectReserveTransition.Execute(reserve, input);

        reserve.IsApproved.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:BR-R-02")]
    public void Submit_WithinTheAutoLimit_ReturnsAnAutoApprovedChangeWithBalances()
    {
        var input = TestReserves.SubmitInput(5_000m, TestReserves.Snapshot());

        var outcome = ReserveChange.Submit(input);

        outcome.Value.ApprovalState.Should().BeOfType<ReserveAutoApprovedState>();
        outcome.Value.Balances.NewBalance.Should().Be(5_000m);
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Submit_AboveTheAutoLimit_LeavesTheChangePending()
    {
        var input = TestReserves.SubmitInput(50_000m, TestReserves.Snapshot());

        var outcome = ReserveChange.Submit(input);

        outcome.Value.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", "RESERVE_AMOUNT_ZERO")]
    public void Submit_WithAZeroAmount_ReturnsTheFindingAndNoChange()
    {
        var input = TestReserves.SubmitInput(0m, TestReserves.Snapshot());

        var outcome = ReserveChange.Submit(input);

        outcome.IsSuccess.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Reject_BySupervisor_CancelsThePostingInTheSameCall()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = new RejectReserveInput(
            TestClaims.Supervisor,
            TestReserves.Approver,
            TestClaims.Later,
            TestReserves.Snapshot())
        {
            Reason = "Not supported by the report",
        };

        reserve.Reject(input);

        reserve.PostingState.Should().BeOfType<ReservePostingCancelledState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Retract_BySubmitter_CancelsThePostingInTheSameCall()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = new RetractReserveInput(
            TestClaims.Handler,
            TestReserves.Submitter,
            TestClaims.Later,
            TestReserves.Snapshot());

        reserve.Retract(input);

        reserve.PostingState.Should().BeOfType<ReservePostingCancelledState>();
    }

    [Fact]
    [Trait("Rule", "RESERVE_APPROVER_ROLE_REQUIRED")]
    public void Reject_ByAHandler_ReturnsTheFindingAndKeepsThePosting()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = new RejectReserveInput(
            TestClaims.Handler,
            TestReserves.Approver,
            TestClaims.Later,
            TestReserves.Snapshot())
        {
            Reason = "Not supported by the report",
        };

        var outcome = reserve.Reject(input);

        outcome.IsSuccess.Should().BeFalse();
        reserve.PostingState.Should().BeOfType<ReservePostingPendingState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Submit_RaisesOneSubmittedEvent()
    {
        var input = TestReserves.SubmitInput(50_000m, TestReserves.Snapshot());

        var outcome = ReserveChange.Submit(input);

        var submitted = outcome.Value.PopEvents().OfType<ReserveSubmittedEvent>().Should().ContainSingle().Subject;
        submitted.Amount.Should().Be(50_000m);
        submitted.ActorId.Should().Be(TestReserves.Submitter);
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Submit_WithinTheAutoLimit_RaisesOneApprovalStateChangedEvent()
    {
        var input = TestReserves.SubmitInput(5_000m, TestReserves.Snapshot());

        var outcome = ReserveChange.Submit(input);

        var events = outcome.Value.PopEvents().OfType<ReserveApprovalStateChangedEvent>().ToList();
        var changed = events.Should().ContainSingle().Subject;
        changed.From.Should().Be(nameof(ReserveApprovalStateKind.PendingApproval));
        changed.To.Should().Be(nameof(ReserveApprovalStateKind.AutoApproved));
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Approve_FromPending_TouchesTheRowAndRaisesOneApprovalStateChangedEvent()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = new ApproveReserveInput(
            TestClaims.Supervisor,
            TestReserves.Approver,
            TestClaims.Later,
            TestReserves.Snapshot());

        reserve.Approve(input);

        reserve.UpdatedAt.Should().Be(TestClaims.Later);
        var changed = reserve.PopEvents().OfType<ReserveApprovalStateChangedEvent>().Should().ContainSingle().Subject;
        changed.From.Should().Be(nameof(ReserveApprovalStateKind.PendingApproval));
        changed.To.Should().Be(nameof(ReserveApprovalStateKind.Approved));
        changed.ActorId.Should().Be(TestReserves.Approver);
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL")]
    public void MarkPosted_FromPending_TouchesTheRowAndRaisesOnePostingStateChangedEvent()
    {
        var reserve = TestReserves.SubmitAutoApproved(5_000m);
        var input = new MarkReservePostedInput(TestClaims.Actor, TestClaims.Later) { JobId = "job-1" };

        reserve.MarkPosted(input);

        reserve.UpdatedAt.Should().Be(TestClaims.Later);
        var changed = reserve.PopEvents().OfType<ReservePostingStateChangedEvent>().Should().ContainSingle().Subject;
        changed.From.Should().Be(nameof(ReservePostingStateKind.Pending));
        changed.To.Should().Be(nameof(ReservePostingStateKind.Posted));
        changed.JobId.Should().Be("job-1");
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Approve_ByAHandler_LeavesTheRowUntouchedAndRaisesNoEvent()
    {
        var reserve = TestReserves.Submit(50_000m);
        var updatedAt = reserve.UpdatedAt;
        var input = new ApproveReserveInput(
            TestClaims.Handler,
            TestReserves.Approver,
            TestClaims.Later,
            TestReserves.Snapshot());

        reserve.Approve(input);

        reserve.UpdatedAt.Should().Be(updatedAt);
        reserve.PopEvents().Should().BeEmpty();
    }
}
