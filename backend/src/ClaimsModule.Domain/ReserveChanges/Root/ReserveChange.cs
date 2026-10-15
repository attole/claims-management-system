using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.ReserveChanges.Root.Bases;
using ClaimsModule.Domain.ReserveChanges.Root.Events;
using ClaimsModule.Domain.ReserveChanges.Root.Operations;
using ClaimsModule.Domain.ReserveChanges.Root.Transitions;

namespace ClaimsModule.Domain.ReserveChanges.Root;

public sealed class ReserveChange : AggregateRoot,
    IAuditable,
    ITenantOwned,
    IStateful<ReserveApprovalState>,
    IStateful<ReservePostingState>
{
    private ReserveChange()
    {
        Details = new ReserveChangeDetails(Guid.Empty, Guid.Empty, ReserveTransactionType.Add, 0m, string.Empty, 0);
        ApprovalState = new ReservePendingApprovalState();
        PostingState = new ReservePostingPendingState();
    }

    internal ReserveChange(
        Guid id,
        Guid organisationId,
        ReserveChangeDetails details,
        Guid submittedBy,
        DateTimeOffset now)
        : base(id)
    {
        OrganisationId = organisationId;
        Details = details;
        ApprovalState = new ReservePendingApprovalState();
        PostingState = new ReservePostingPendingState();
        SubmittedBy = submittedBy;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public ReserveChangeDetails Details { get; private set; }

    public ReserveApprovalState ApprovalState { get; internal set; }

    public ReservePostingState PostingState { get; internal set; }

    public ReservePostingDetails PostingDetails { get; internal set; } = ReservePostingDetails.Empty;

    public ReserveBalances Balances { get; internal set; } = ReserveBalances.Empty;

    public ReserveReview Review { get; internal set; } = ReserveReview.Empty;

    public Guid SubmittedBy { get; private set; } = Guid.Empty;

    public string IdempotencyKey => $"Reserve:{Details.ReserveId}:Change:{Details.ChangeSequence}";

    public bool IsApproved => ApprovalState.IsApproved;

    public bool IsRejected => ApprovalState is ReserveRejectedState;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid UserCreated { get; private set; } = Guid.Empty;

    public Guid UserModified { get; private set; } = Guid.Empty;

    public Guid OrganisationId { get; private set; } = Guid.Empty;

    ReserveApprovalState IStateful<ReserveApprovalState>.State => ApprovalState;

    ReservePostingState IStateful<ReservePostingState>.State => PostingState;

    public static FlowOutcome<ReserveChange, DomainFinding> Submit(SubmitReserveInput input)
    {
        var outcome = new SubmitReserveOperation().Execute(input);

        if (!outcome.IsSuccess)
        {
            return outcome;
        }

        var reserve = outcome.Value;
        var submitted = new ReserveSubmittedEvent(
            Ids.Next(input.Now),
            input.Now,
            input.ActorId,
            reserve.Id,
            reserve.Details.ReserveId,
            reserve.Details.ClaimId,
            reserve.Details.Amount);
        reserve.Raise(submitted);
        reserve.AutoApprove(input);

        return outcome;
    }

    public FlowOutcome<ReserveApprovalState, DomainFinding> Approve(ApproveReserveInput input)
    {
        var from = ApprovalState;
        var outcome = new ApproveReserveTransition().Execute(this, input);

        if (outcome.IsSuccess)
        {
            RecordApproval(from, input);
        }

        return outcome;
    }

    public FlowOutcome<ReserveApprovalState, DomainFinding> Reject(RejectReserveInput input)
    {
        var from = ApprovalState;
        var outcome = new RejectReserveTransition().Execute(this, input);

        if (outcome.IsSuccess)
        {
            RecordApproval(from, input);
            CancelPosting(input);
        }

        return outcome;
    }

    public FlowOutcome<ReserveApprovalState, DomainFinding> Retract(RetractReserveInput input)
    {
        var from = ApprovalState;
        var outcome = new RetractReserveTransition().Execute(this, input);

        if (outcome.IsSuccess)
        {
            RecordApproval(from, input);
            CancelPosting(input);
        }

        return outcome;
    }

    public FlowOutcome<ReservePostingState, DomainFinding> MarkPosted(MarkReservePostedInput input)
    {
        var from = PostingState;
        var outcome = new MarkReservePostedTransition().Execute(this, input);

        if (outcome.IsSuccess)
        {
            RecordPosting(from, input);
        }

        return outcome;
    }

    public FlowOutcome<ReservePostingState, DomainFinding> MarkPostingFailed(MarkReservePostingFailedInput input)
    {
        var from = PostingState;
        var outcome = new MarkReservePostingFailedTransition().Execute(this, input);

        if (outcome.IsSuccess)
        {
            RecordPosting(from, input);
        }

        return outcome;
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
    }

    private void AutoApprove(SubmitReserveInput input)
    {
        var autoApprove = new AutoApproveReserveInput(input.ActorRole, input.ActorId, input.Now, input.Snapshot);
        var from = ApprovalState;
        var outcome = new AutoApproveReserveTransition().Execute(this, autoApprove);

        if (outcome.IsSuccess)
        {
            RecordApproval(from, autoApprove);
        }
    }

    private void CancelPosting(ReserveChangeBaseTransitionInput input)
    {
        var cancel = new CancelReservePostingInput(input.ActorId, input.Now);
        var from = PostingState;
        var outcome = new CancelReservePostingTransition().Execute(this, cancel);

        if (outcome.IsSuccess)
        {
            RecordPosting(from, cancel);
        }
    }

    private void RecordApproval(ReserveApprovalState from, ReserveChangeBaseTransitionInput input)
    {
        var changed = new ReserveApprovalStateChangedEvent(
            Ids.Next(input.Now),
            input.Now,
            input.ActorId,
            Id,
            Details.ReserveId,
            Details.ClaimId,
            from.Name,
            ApprovalState.Name,
            input.Reason);
        Touch(input.Now);
        Raise(changed);
    }

    private void RecordPosting(ReservePostingState from, ReserveChangeBasePostingTransitionInput input)
    {
        var changed = new ReservePostingStateChangedEvent(
            Ids.Next(input.Now),
            input.Now,
            input.ActorId,
            Id,
            Details.ReserveId,
            Details.ClaimId,
            from.Name,
            PostingState.Name,
            PostingDetails.JobId,
            PostingDetails.FailureReason);
        Touch(input.Now);
        Raise(changed);
    }
}
