using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Events;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Roles;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History;

public sealed class ReserveHistoryTests
{
    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Reject_BySupervisor_CancelsThePostingInTheSameCall()
    {
        var history = TestReserves.Submit(50_000m);
        var input = RejectInput(TestClaims.Supervisor);

        history.Reject(input);

        history.PostingState.Should().BeOfType<ReservePostingCancelledState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Retract_BySubmitter_CancelsThePostingInTheSameCall()
    {
        var history = TestReserves.Submit(50_000m);
        var input = RetractInput();

        history.Retract(input);

        history.PostingState.Should().BeOfType<ReservePostingCancelledState>();
    }

    [Fact]
    [Trait("Rule", HasRejectionAuthority.Code)]
    public void Reject_ByAHandler_ReturnsTheFindingAndKeepsThePosting()
    {
        var history = TestReserves.Submit(50_000m);
        var input = RejectInput(TestClaims.Handler);

        var outcome = history.Reject(input);

        outcome.IsSuccess.Should().BeFalse();
        history.PostingState.Should().BeOfType<ReservePostingPendingState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void AutoApprove_WithinTheLimit_RaisesOneApprovalStateChangedEvent()
    {
        var history = TestReserves.Submit(5_000m);
        var input = TestReserves.AutoApproveInput();

        history.AutoApprove(input);

        var events = history.PopEvents().OfType<ReserveHistoryApprovalStateChangedEvent>();
        var changed = events.Should().ContainSingle().Subject;
        changed.From.Should().Be(nameof(ReserveApprovalStateKind.PendingApproval));
        changed.To.Should().Be(nameof(ReserveApprovalStateKind.AutoApproved));
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Approve_FromPending_RaisesOneApprovalStateChangedEvent()
    {
        var history = TestReserves.Submit(50_000m);
        var input = TestReserves.ApproveInput(TestClaims.Supervisor, TestReserves.Approver);

        history.Approve(input);

        var events = history.PopEvents().OfType<ReserveHistoryApprovalStateChangedEvent>();
        var changed = events.Should().ContainSingle().Subject;
        changed.From.Should().Be(nameof(ReserveApprovalStateKind.PendingApproval));
        changed.To.Should().Be(nameof(ReserveApprovalStateKind.Approved));
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL")]
    public void MarkPosted_FromPending_RaisesOnePostingStateChangedEvent()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var input = new RowPostingInput() { JobId = "job-1" };

        history.MarkPosted(input);

        var events = history.PopEvents().OfType<ReserveHistoryPostingStateChangedEvent>();
        var changed = events.Should().ContainSingle().Subject;
        changed.From.Should().Be(nameof(ReservePostingStateKind.Pending));
        changed.To.Should().Be(nameof(ReservePostingStateKind.Posted));
        changed.JobId.Should().Be("job-1");
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Reject_RaisesOneApprovalStateChangedEvent()
    {
        var history = TestReserves.Submit(50_000m);
        var input = RejectInput(TestClaims.Supervisor);

        history.Reject(input);

        var events = history.PopEvents().OfType<ReserveHistoryApprovalStateChangedEvent>();
        var changed = events.Should().ContainSingle().Subject;
        changed.To.Should().Be(nameof(ReserveApprovalStateKind.Rejected));
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Retract_RaisesOneApprovalStateChangedEvent()
    {
        var history = TestReserves.Submit(50_000m);
        var input = RetractInput();

        history.Retract(input);

        var events = history.PopEvents().OfType<ReserveHistoryApprovalStateChangedEvent>();
        var changed = events.Should().ContainSingle().Subject;
        changed.To.Should().Be(nameof(ReserveApprovalStateKind.Cancelled));
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void Retract_RaisesThePostingCancelledEvent()
    {
        var history = TestReserves.Submit(50_000m);
        var input = RetractInput();

        history.Retract(input);

        var events = history.PopEvents().OfType<ReserveHistoryPostingStateChangedEvent>();
        var changed = events.Should().ContainSingle().Subject;
        changed.To.Should().Be(nameof(ReservePostingStateKind.Cancelled));
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void MarkPostingFailed_RaisesOnePostingStateChangedEvent()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var input = new RowPostingInput { JobId = "job-1", FailureReason = "Ledger unavailable" };

        history.MarkPostingFailed(input);

        var events = history.PopEvents().OfType<ReserveHistoryPostingStateChangedEvent>();
        var changed = events.Should().ContainSingle().Subject;
        changed.To.Should().Be(nameof(ReservePostingStateKind.Failed));
        changed.FailureReason.Should().Be("Ledger unavailable");
    }

    [Fact]
    public void IsSubmittedBy_TheSubmitter_IsTrue()
    {
        var history = TestReserves.Submit(50_000m);

        var result = history.IsSubmittedBy(TestReserves.Submitter);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsSubmittedBy_AnotherUser_IsFalse()
    {
        var history = TestReserves.Submit(50_000m);

        var result = history.IsSubmittedBy(TestReserves.Approver);

        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public void RetryPosting_RaisesOnePostingStateChangedEvent()
    {
        var history = TestReserves.SubmitAutoApproved(5_000m);
        var failed = new RowPostingInput { JobId = "job-1", FailureReason = "timeout" };
        history.MarkPostingFailed(failed);
        history.PopEvents();
        var input = new RowPostingInput { JobId = "job-2" };

        history.RetryPosting(input);

        var events = history.PopEvents().OfType<ReserveHistoryPostingStateChangedEvent>();
        var changed = events.Should().ContainSingle().Subject;
        changed.To.Should().Be(nameof(ReservePostingStateKind.Pending));
    }

    private static RejectRowInput RejectInput(UserRole role)
    {
        var input = new RejectRowInput(role, TestReserves.Approver, TestClaims.Later)
        {
            Reason = "Not supported by the report",
        };

        return input;
    }

    private static RowDecisionInput RetractInput()
    {
        var input = new RowDecisionInput(TestReserves.Submitter, TestClaims.Later);

        return input;
    }
}
