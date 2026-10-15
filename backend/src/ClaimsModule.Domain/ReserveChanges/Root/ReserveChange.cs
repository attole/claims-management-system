using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.ReserveChanges.Root.Events;

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

    private ReserveChange(
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

    public ReserveApprovalState ApprovalState { get; private set; }

    public ReservePostingState PostingState { get; private set; }

    public ReservePostingDetails PostingDetails { get; private set; } = ReservePostingDetails.Empty;

    public ReserveBalances Balances { get; private set; } = ReserveBalances.Empty;

    public ReserveReview Review { get; private set; } = ReserveReview.Empty;

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

    internal static ReserveChange Submit(
        Guid organisationId,
        ReserveChangeDetails details,
        Guid actorId,
        DateTimeOffset now)
    {
        var reserve = new ReserveChange(Ids.Next(now), organisationId, details, actorId, now);
        var submitted = new ReserveSubmitted(
            Ids.Next(now),
            now,
            actorId,
            reserve.Id,
            details.ReserveId,
            details.ClaimId,
            details.Amount);
        reserve.Raise(submitted);

        return reserve;
    }

    internal void ChangeApprovalState(
        ReserveApprovalState approval,
        string reason,
        Guid actorId,
        decimal approvedBalance,
        DateTimeOffset now)
    {
        var from = ApprovalState;
        Review = new ReserveReview(actorId, now, reason);
        ApprovalState = approval;

        if (approval.IsApproved)
        {
            Balances = new ReserveBalances(approvedBalance, approvedBalance + Details.Amount);
        }

        Touch(now);
        var changed = new ReserveApprovalStateChanged(
            Ids.Next(now),
            now,
            actorId,
            Id,
            Details.ReserveId,
            Details.ClaimId,
            from.Name,
            approval.Name,
            reason);
        Raise(changed);
    }

    internal void ChangePostingState(
        ReservePostingState posting,
        ReservePostingDetails details,
        Guid actorId,
        DateTimeOffset now)
    {
        var from = PostingState;
        PostingState = posting;
        PostingDetails = details;
        Touch(now);
        var changed = new ReservePostingStateChanged(
            Ids.Next(now),
            now,
            actorId,
            Id,
            Details.ReserveId,
            Details.ClaimId,
            from.Name,
            posting.Name,
            details.JobId,
            details.FailureReason);
        Raise(changed);
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
    }
}
