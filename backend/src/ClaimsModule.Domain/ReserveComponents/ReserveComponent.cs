using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents.Events;
using ClaimsModule.Domain.ReserveComponents.Flows;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Events;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.Inputs;

namespace ClaimsModule.Domain.ReserveComponents;

public sealed class ReserveComponent : AggregateRoot, ISoftDeletable, IStateful<ReserveComponentState>
{
    private readonly List<ReserveHistory> _history = [];

    private ReserveComponent()
    {
        ComponentType = new ReserveIndemnityComponentType();
        State = new ReserveComponentActiveState();
    }

    internal ReserveComponent(
        Guid id,
        Guid claimId,
        Guid organisationId,
        ReserveComponentType componentType,
        string notes)
        : base(id)
    {
        OrganisationId = organisationId;
        ClaimId = claimId;
        ComponentType = componentType;
        State = new ReserveComponentActiveState();
        Notes = notes;
    }

    public Guid ClaimId { get; private set; } = Guid.Empty;

    public ReserveComponentType ComponentType { get; private set; }

    public ReserveComponentState State { get; internal set; }

    public string Notes { get; private set; } = string.Empty;

    public IReadOnlyList<ReserveHistory> History => _history.AsReadOnly();

    public decimal ApprovedBalance => _history
        .Where(history => history.ApprovalState.IsApproved)
        .Sum(history => history.Details.Amount);

    public int NextChangeSequence => _history
        .Select(history => history.Details.ChangeSequence)
        .DefaultIfEmpty(0)
        .Max() + 1;

    public bool IsDeleted { get; private set; } = false;

    public static Outcome<ReserveComponent> Open(OpenReserveComponentInput input)
    {
        return OpenReserveComponentOperation.Run(input);
    }

    public Outcome<ReserveHistory> SubmitHistory(SubmitHistoryInput input)
    {
        return SubmitHistoryOperation.Run(this, input)
            .OnSuccess(history => RecordSubmission(history))
            .OnSuccess(history => AutoApprove(history, input));
    }

    public Outcome<Move<ReserveApprovalState>> ApproveHistory(ApproveHistoryInput input)
    {
        return ApproveHistoryOnReserveComponentOperation.Run(this, input);
    }

    public Outcome<Move<ReserveApprovalState>> RejectHistory(RejectHistoryInput input)
    {
        return RejectHistoryOnReserveComponentOperation.Run(this, input);
    }

    public Outcome<Move<ReserveApprovalState>> RetractHistory(RetractHistoryInput input)
    {
        return RetractHistoryOnReserveComponentOperation.Run(this, input);
    }

    public Outcome<Move<ReservePostingState>> MarkHistoryPosted(HistoryPostingInput input)
    {
        return MarkHistoryPostedOnReserveComponentOperation.Run(this, input);
    }

    public Outcome<Move<ReservePostingState>> RetryHistoryPosting(HistoryPostingInput input)
    {
        return RetryHistoryPostingOnReserveComponentOperation.Run(this, input);
    }

    public Outcome<Move<ReservePostingState>> MarkHistoryPostingFailed(HistoryPostingInput input)
    {
        return MarkHistoryPostingFailedOnReserveComponentOperation.Run(this, input);
    }

    public Outcome<Move<ReserveComponentState>> Close()
    {
        return CloseReserveComponentTransition.Run(this, NoInput.Value)
            .OnSuccess(move => RecordMove(move));
    }

    public Outcome<Move<ReserveComponentState>> Reopen()
    {
        return ReopenReserveComponentTransition.Run(this, NoInput.Value)
            .OnSuccess(move => RecordMove(move));
    }

    public bool CanHoldBalanceAfter(decimal amount)
    {
        bool canHold = ComponentType.CanHoldBalance(ApprovedBalance + amount);

        return canHold;
    }

    internal void AttachHistory(ReserveHistory history)
    {
        _history.Add(history);
    }

    private void RecordMove(Move<ReserveComponentState> move)
    {
        var changed = new ReserveComponentStateChangedEvent(Id, ClaimId, move.From.Name, move.To.Name);
        Raise(changed);
    }

    private void RecordSubmission(ReserveHistory history)
    {
        var submitted = new ReserveHistorySubmittedEvent(history.Id, Id, ClaimId, history.Details.Amount);
        Raise(submitted);
    }

    private void AutoApprove(ReserveHistory history, SubmitHistoryInput input)
    {
        var autoApprove = new AutoApproveRowInput(
            input.ClaimApprovedTotal,
            ApprovedBalance,
            input.ActorId,
            input.Now);
        history.AutoApprove(autoApprove);
    }
}
