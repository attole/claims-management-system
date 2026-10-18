using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Events;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;

namespace ClaimsModule.Domain.ReserveComponents.History;

public sealed class ReserveHistory : Entity, IStateful<ReserveApprovalState>, IStateful<ReservePostingState>
{
    private ReserveHistory()
    {
        Details = new ReserveHistoryDetails(
            Guid.Empty,
            new ReserveIndemnityComponentType(),
            ReserveTransactionType.Add,
            0m,
            string.Empty,
            0);
        ApprovalState = new ReservePendingApprovalState();
        PostingState = new ReservePostingPendingState();
    }

    internal ReserveHistory(
        Guid id,
        Guid reserveComponentId,
        Guid organisationId,
        ReserveHistoryDetails details,
        Guid submittedByUserId,
        DateTimeOffset submittedAt)
        : base(id)
    {
        ReserveComponentId = reserveComponentId;
        OrganisationId = organisationId;
        Details = details;
        ApprovalState = new ReservePendingApprovalState();
        PostingState = new ReservePostingPendingState();
        SubmittedByUserId = submittedByUserId;
        SubmittedAt = submittedAt;
    }

    public Guid ReserveComponentId { get; private set; } = Guid.Empty;

    public ReserveHistoryDetails Details { get; private set; }

    public ReserveApprovalState ApprovalState { get; internal set; }

    public ReservePostingState PostingState { get; internal set; }

    public ReservePostingDetails PostingDetails { get; internal set; } = ReservePostingDetails.Empty;

    public ReserveBalances Balances { get; internal set; } = ReserveBalances.Empty;

    public ReserveReview Review { get; internal set; } = ReserveReview.Empty;

    public Guid SubmittedByUserId { get; private set; } = Guid.Empty;

    public DateTimeOffset SubmittedAt { get; private set; } = DateTimeOffset.MinValue;

    public string IdempotencyKey => $"Reserve:{ReserveComponentId}:Change:{Details.ChangeSequence}";

    ReserveApprovalState IStateful<ReserveApprovalState>.State => ApprovalState;

    ReservePostingState IStateful<ReservePostingState>.State => PostingState;

    public bool IsSubmittedBy(Guid actorId)
    {
        bool isSubmitter = SubmittedByUserId == actorId;

        return isSubmitter;
    }

    internal Outcome<Move<ReserveApprovalState>> AutoApprove(AutoApproveRowInput input)
    {
        return AutoApproveHistoryTransition.Run(this, input)
            .OnSuccess(move => RecordApproval(move, input));
    }

    internal Outcome<Move<ReserveApprovalState>> Approve(ApproveRowInput input)
    {
        return ApproveHistoryTransition.Run(this, input)
            .OnSuccess(move => RecordApproval(move, input));
    }

    internal Outcome<Move<ReserveApprovalState>> Reject(RejectRowInput input)
    {
        return RejectHistoryTransition.Run(this, input)
            .OnSuccess(move => RecordApproval(move, input))
            .OnSuccess(() => CancelPosting());
    }

    internal Outcome<Move<ReserveApprovalState>> Retract(RowDecisionInput input)
    {
        return RetractHistoryTransition.Run(this, input)
            .OnSuccess(move => RecordApproval(move, input))
            .OnSuccess(() => CancelPosting());
    }

    internal Outcome<Move<ReservePostingState>> MarkPosted(RowPostingInput input)
    {
        return MarkHistoryPostedTransition.Run(this, input)
            .OnSuccess(move => RecordPosting(move));
    }

    internal Outcome<Move<ReservePostingState>> RetryPosting(RowPostingInput input)
    {
        return RetryHistoryPostingTransition.Run(this, input)
            .OnSuccess(move => RecordPosting(move));
    }

    internal Outcome<Move<ReservePostingState>> MarkPostingFailed(RowPostingInput input)
    {
        return MarkHistoryPostingFailedTransition.Run(this, input)
            .OnSuccess(move => RecordPosting(move));
    }

    private void CancelPosting()
    {
        var cancel = new RowPostingInput();
        CancelHistoryPostingTransition.Run(this, cancel).OnSuccess(move => RecordPosting(move));
    }

    private void RecordApproval(Move<ReserveApprovalState> move, RowDecisionInput input)
    {
        var changed = new ReserveHistoryApprovalStateChangedEvent(
            Id,
            ReserveComponentId,
            Details.ClaimId,
            move.From.Name,
            move.To.Name,
            input.Reason);
        Raise(changed);
    }

    private void RecordPosting(Move<ReservePostingState> move)
    {
        var changed = new ReserveHistoryPostingStateChangedEvent(
            Id,
            ReserveComponentId,
            Details.ClaimId,
            move.From.Name,
            move.To.Name,
            PostingDetails.JobId,
            PostingDetails.FailureReason);
        Raise(changed);
    }
}
