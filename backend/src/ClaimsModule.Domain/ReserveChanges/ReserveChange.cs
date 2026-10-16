using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveChanges.Events;
using ClaimsModule.Domain.ReserveChanges.Flows;
using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.Reserves;

namespace ClaimsModule.Domain.ReserveChanges;

public sealed class ReserveChange : AggregateRoot, IStateful<ReserveApprovalState>, IStateful<ReservePostingState>
{
    private ReserveChange()
    {
        Details = new ReserveChangeDetails(
            Guid.Empty,
            Guid.Empty,
            new ReserveIndemnityComponentType(),
            ReserveTransactionType.Add,
            0m,
            string.Empty,
            0);
        ApprovalState = new ReservePendingApprovalState();
        PostingState = new ReservePostingPendingState();
    }

    internal ReserveChange(
        Guid id,
        Guid organisationId,
        ReserveChangeDetails details,
        Guid submittedBy,
        DateTimeOffset submittedAt)
        : base(id)
    {
        OrganisationId = organisationId;
        Details = details;
        ApprovalState = new ReservePendingApprovalState();
        PostingState = new ReservePostingPendingState();
        SubmittedBy = submittedBy;
        SubmittedAt = submittedAt;
    }

    public ReserveChangeDetails Details { get; private set; }

    public ReserveApprovalState ApprovalState { get; internal set; }

    public ReservePostingState PostingState { get; internal set; }

    public ReservePostingDetails PostingDetails { get; internal set; } = ReservePostingDetails.Empty;

    public ReserveBalances Balances { get; internal set; } = ReserveBalances.Empty;

    public ReserveReview Review { get; internal set; } = ReserveReview.Empty;

    public Guid SubmittedBy { get; private set; } = Guid.Empty;

    public DateTimeOffset SubmittedAt { get; private set; } = DateTimeOffset.MinValue;

    public string IdempotencyKey => $"Reserve:{Details.ReserveId}:Change:{Details.ChangeSequence}";

    ReserveApprovalState IStateful<ReserveApprovalState>.State => ApprovalState;

    ReservePostingState IStateful<ReservePostingState>.State => PostingState;

    public static Outcome<ReserveChange> Submit(SubmitReserveInput input)
    {
        return SubmitReserveOperation.Run(input)
            .OnSuccess(change => change.RecordSubmission())
            .OnSuccess(change => change.AutoApprove(input));
    }

    public Outcome<Move<ReserveApprovalState>> Approve(ApproveReserveInput input)
    {
        return ApproveReserveTransition.Run(this, input)
            .OnSuccess(move => RecordApproval(move, input));
    }

    public Outcome<Move<ReserveApprovalState>> Reject(RejectReserveInput input)
    {
        return RejectReserveTransition.Run(this, input)
            .OnSuccess(move => RecordApproval(move, input))
            .OnSuccess(() => CancelPosting());
    }

    public Outcome<Move<ReserveApprovalState>> Retract(ReserveDecisionInput input)
    {
        return RetractReserveTransition.Run(this, input)
            .OnSuccess(move => RecordApproval(move, input))
            .OnSuccess(() => CancelPosting());
    }

    public Outcome<Move<ReservePostingState>> MarkPosted(ReservePostingInput input)
    {
        return MarkReservePostedTransition.Run(this, input)
            .OnSuccess(move => RecordPosting(move));
    }

    public Outcome<Move<ReservePostingState>> RetryPosting(ReservePostingInput input)
    {
        return RetryReservePostingTransition.Run(this, input)
            .OnSuccess(move => RecordPosting(move));
    }

    public Outcome<Move<ReservePostingState>> MarkPostingFailed(ReservePostingInput input)
    {
        return MarkReservePostingFailedTransition.Run(this, input)
            .OnSuccess(move => RecordPosting(move));
    }

    public bool IsSubmittedBy(Guid actorId)
    {
        bool isSubmitter = SubmittedBy == actorId;

        return isSubmitter;
    }

    private void RecordSubmission()
    {
        var submitted = new ReserveSubmittedEvent(Id, Details.ReserveId, Details.ClaimId, Details.Amount);
        Raise(submitted);
    }

    private void AutoApprove(SubmitReserveInput input)
    {
        var autoApprove = new AutoApproveReserveInput(
            input.ClaimApprovedTotal,
            input.ReserveApprovedBalance,
            input.ActorId,
            input.Now);
        AutoApproveReserveTransition.Run(this, autoApprove)
            .OnSuccess(move => RecordApproval(move, autoApprove));
    }

    private void CancelPosting()
    {
        var cancel = new ReservePostingInput();
        CancelReservePostingTransition.Run(this, cancel).OnSuccess(move => RecordPosting(move));
    }

    private void RecordApproval(Move<ReserveApprovalState> move, ReserveDecisionInput input)
    {
        var changed = new ReserveApprovalStateChangedEvent(
            Id,
            Details.ReserveId,
            Details.ClaimId,
            move.From.Name,
            move.To.Name,
            input.Reason);
        Raise(changed);
    }

    private void RecordPosting(Move<ReservePostingState> move)
    {
        var changed = new ReservePostingStateChangedEvent(
            Id,
            Details.ReserveId,
            Details.ClaimId,
            move.From.Name,
            move.To.Name,
            PostingDetails.JobId,
            PostingDetails.FailureReason);
        Raise(changed);
    }
}
