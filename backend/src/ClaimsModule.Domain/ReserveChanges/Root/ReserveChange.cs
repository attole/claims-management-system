using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.ReserveChanges.Root.Events;
using ClaimsModule.Domain.ReserveChanges.Root.Flows;
using ClaimsModule.Domain.ReserveChanges.Root.Inputs;

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
        return SubmitReserveOperation.Execute(input)
            .OnSuccess(reserve => reserve.Raise(new ReserveSubmittedEvent(
                input.Stamp(),
                reserve.Id,
                reserve.Details.ReserveId,
                reserve.Details.ClaimId,
                reserve.Details.Amount)))
            .OnSuccess(reserve => reserve.AutoApprove(input));
    }

    public FlowOutcome<StateChange<ReserveApprovalState>, DomainFinding> Approve(ApproveReserveInput input)
    {
        return ApproveReserveTransition.Execute(this, input)
            .OnSuccess(change => RecordApproval(change, input))
            .OnSuccess(() => Settle(input));
    }

    public FlowOutcome<StateChange<ReserveApprovalState>, DomainFinding> Reject(RejectReserveInput input)
    {
        return RejectReserveTransition.Execute(this, input)
            .OnSuccess(change => RecordApproval(change, input))
            .OnSuccess(() => CancelPosting(input))
            .OnSuccess(() => Settle(input));
    }

    public FlowOutcome<StateChange<ReserveApprovalState>, DomainFinding> Retract(RetractReserveInput input)
    {
        return RetractReserveTransition.Execute(this, input)
            .OnSuccess(change => RecordApproval(change, input))
            .OnSuccess(() => CancelPosting(input))
            .OnSuccess(() => Settle(input));
    }

    public FlowOutcome<StateChange<ReservePostingState>, DomainFinding> MarkPosted(MarkReservePostedInput input)
    {
        return MarkReservePostedTransition.Execute(this, input)
            .OnSuccess(change => RecordPosting(change, input))
            .OnSuccess(() => Settle(input));
    }

    public FlowOutcome<StateChange<ReservePostingState>, DomainFinding> MarkPostingFailed(
        MarkReservePostingFailedInput input)
    {
        return MarkReservePostingFailedTransition.Execute(this, input)
            .OnSuccess(change => RecordPosting(change, input))
            .OnSuccess(() => Settle(input));
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
    }

    private void Settle(ReserveChangeBaseOperationInput input)
    {
        Touch(input.Now);
    }

    private void AutoApprove(SubmitReserveInput input)
    {
        var autoApprove = new AutoApproveReserveInput(input.ActorRole, input.ActorId, input.Now, input.Snapshot);
        AutoApproveReserveTransition.Execute(this, autoApprove)
            .OnSuccess(change => RecordApproval(change, autoApprove))
            .OnSuccess(() => Settle(autoApprove));
    }

    private void CancelPosting(ReserveChangeBaseTransitionInput input)
    {
        var cancel = new CancelReservePostingInput(input.ActorId, input.Now);
        CancelReservePostingTransition.Execute(this, cancel).OnSuccess(change => RecordPosting(change, cancel));
    }

    private void RecordApproval(StateChange<ReserveApprovalState> change, ReserveChangeBaseTransitionInput input)
    {
        Raise(new ReserveApprovalStateChangedEvent(
            input.Stamp(),
            Id,
            Details.ReserveId,
            Details.ClaimId,
            change.From.Name,
            change.To.Name,
            input.Reason));
    }

    private void RecordPosting(StateChange<ReservePostingState> change, ReserveChangeBasePostingTransitionInput input)
    {
        Raise(new ReservePostingStateChangedEvent(
            input.Stamp(),
            Id,
            Details.ReserveId,
            Details.ClaimId,
            change.From.Name,
            change.To.Name,
            PostingDetails.JobId,
            PostingDetails.FailureReason));
    }
}
