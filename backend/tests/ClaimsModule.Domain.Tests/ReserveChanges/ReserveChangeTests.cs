using ClaimsModule.Domain.ReserveChanges;
using ClaimsModule.Domain.ReserveChanges.Events;
using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Roles;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges;

public sealed class ReserveChangeTests
{
    [Fact]
    [Trait("Rule", "S:BR-R-02")]
    public void Submit_WithinTheAutoLimit_ReturnsAnAutoApprovedChangeWithBalances()
    {
        var input = TestReserves.SubmitInput(5_000m);

        var outcome = ReserveChange.Submit(input);

        outcome.Value.ApprovalState.Should().BeOfType<ReserveAutoApprovedState>();
        outcome.Value.Balances.NewBalance.Should().Be(5_000m);
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Submit_AboveTheAutoLimit_LeavesTheChangePending()
    {
        var input = TestReserves.SubmitInput(50_000m);

        var outcome = ReserveChange.Submit(input);

        outcome.Value.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Reject_BySupervisor_CancelsThePostingInTheSameCall()
    {
        var change = TestReserves.Submit(50_000m);
        var input = RejectInput(TestClaims.Supervisor);

        change.Reject(input);

        change.PostingState.Should().BeOfType<ReservePostingCancelledState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Retract_BySubmitter_CancelsThePostingInTheSameCall()
    {
        var change = TestReserves.Submit(50_000m);
        var input = RetractInput();

        change.Retract(input);

        change.PostingState.Should().BeOfType<ReservePostingCancelledState>();
    }

    [Fact]
    [Trait("Rule", HasRejectionAuthority.Code)]
    public void Reject_ByAHandler_ReturnsTheFindingAndKeepsThePosting()
    {
        var change = TestReserves.Submit(50_000m);
        var input = RejectInput(TestClaims.Handler);

        var outcome = change.Reject(input);

        outcome.IsSuccess.Should().BeFalse();
        change.PostingState.Should().BeOfType<ReservePostingPendingState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Submit_RaisesOneSubmittedEvent()
    {
        var input = TestReserves.SubmitInput(50_000m);

        var outcome = ReserveChange.Submit(input);

        var submitted = outcome.Value.PopEvents().OfType<ReserveSubmittedEvent>().Should().ContainSingle().Subject;
        submitted.Amount.Should().Be(50_000m);
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Submit_WithinTheAutoLimit_RaisesOneApprovalStateChangedEvent()
    {
        var input = TestReserves.SubmitInput(5_000m);

        var outcome = ReserveChange.Submit(input);

        var events = outcome.Value.PopEvents().OfType<ReserveApprovalStateChangedEvent>().ToList();
        var changed = events.Should().ContainSingle().Subject;
        changed.From.Should().Be(nameof(ReserveApprovalStateKind.PendingApproval));
        changed.To.Should().Be(nameof(ReserveApprovalStateKind.AutoApproved));
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Approve_FromPending_RaisesOneApprovalStateChangedEvent()
    {
        var change = TestReserves.Submit(50_000m);
        var input = TestReserves.ApproveInput(TestClaims.Supervisor, TestReserves.Approver);

        change.Approve(input);

        var changed = change.PopEvents().OfType<ReserveApprovalStateChangedEvent>().Should().ContainSingle().Subject;
        changed.From.Should().Be(nameof(ReserveApprovalStateKind.PendingApproval));
        changed.To.Should().Be(nameof(ReserveApprovalStateKind.Approved));
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL")]
    public void MarkPosted_FromPending_RaisesOnePostingStateChangedEvent()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var input = new ReservePostingInput() { JobId = "job-1" };

        change.MarkPosted(input);

        var changed = change.PopEvents().OfType<ReservePostingStateChangedEvent>().Should().ContainSingle().Subject;
        changed.From.Should().Be(nameof(ReservePostingStateKind.Pending));
        changed.To.Should().Be(nameof(ReservePostingStateKind.Posted));
        changed.JobId.Should().Be("job-1");
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Reject_RaisesOneApprovalStateChangedEvent()
    {
        var change = TestReserves.Submit(50_000m);
        var input = RejectInput(TestClaims.Supervisor);

        change.Reject(input);

        var changed = change.PopEvents().OfType<ReserveApprovalStateChangedEvent>().Should().ContainSingle().Subject;
        changed.To.Should().Be(nameof(ReserveApprovalStateKind.Rejected));
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Retract_RaisesOneApprovalStateChangedEvent()
    {
        var change = TestReserves.Submit(50_000m);
        var input = RetractInput();

        change.Retract(input);

        var changed = change.PopEvents().OfType<ReserveApprovalStateChangedEvent>().Should().ContainSingle().Subject;
        changed.To.Should().Be(nameof(ReserveApprovalStateKind.Cancelled));
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Retract_RaisesThePostingCancelledEvent()
    {
        var change = TestReserves.Submit(50_000m);
        var input = RetractInput();

        change.Retract(input);

        var changed = change.PopEvents().OfType<ReservePostingStateChangedEvent>().Should().ContainSingle().Subject;
        changed.To.Should().Be(nameof(ReservePostingStateKind.Cancelled));
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void MarkPostingFailed_RaisesOnePostingStateChangedEvent()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var input = new ReservePostingInput { JobId = "job-1", FailureReason = "Ledger unavailable" };

        change.MarkPostingFailed(input);

        var changed = change.PopEvents().OfType<ReservePostingStateChangedEvent>().Should().ContainSingle().Subject;
        changed.To.Should().Be(nameof(ReservePostingStateKind.Failed));
        changed.FailureReason.Should().Be("Ledger unavailable");
    }

    [Fact]
    public void IsSubmittedBy_TheSubmitter_IsTrue()
    {
        var change = TestReserves.Submit(50_000m);

        var result = change.IsSubmittedBy(TestReserves.Submitter);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsSubmittedBy_AnotherUser_IsFalse()
    {
        var change = TestReserves.Submit(50_000m);

        var result = change.IsSubmittedBy(TestReserves.Approver);

        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void RetryPosting_RaisesOnePostingStateChangedEvent()
    {
        var change = TestReserves.SubmitAutoApproved(5_000m);
        var failed = new ReservePostingInput { JobId = "job-1", FailureReason = "timeout" };
        change.MarkPostingFailed(failed);
        change.PopEvents();
        var input = new ReservePostingInput { JobId = "job-2" };

        change.RetryPosting(input);

        var changed = change.PopEvents().OfType<ReservePostingStateChangedEvent>().Should().ContainSingle().Subject;
        changed.To.Should().Be(nameof(ReservePostingStateKind.Pending));
    }

    private static RejectReserveInput RejectInput(UserRole role)
    {
        var input = new RejectReserveInput(role, TestReserves.Approver, TestClaims.Later)
        {
            Reason = "Not supported by the report",
        };

        return input;
    }

    private static ReserveDecisionInput RetractInput()
    {
        var input = new ReserveDecisionInput(TestReserves.Submitter, TestClaims.Later);

        return input;
    }
}
