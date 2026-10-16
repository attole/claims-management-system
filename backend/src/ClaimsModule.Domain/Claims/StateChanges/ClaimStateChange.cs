using ClaimsModule.Domain.Common.Entities;

namespace ClaimsModule.Domain.Claims.StateChanges;

public sealed class ClaimStateChange : Entity
{
    private ClaimStateChange()
    {
        From = new ClaimDraftState();
        To = new ClaimDraftState();
        Reason = StateChangeReason.Empty;
    }

    internal ClaimStateChange(
        Guid id,
        Guid claimId,
        Guid organisationId,
        ClaimState from,
        ClaimState to,
        StateChangeReason reason,
        string note)
        : base(id)
    {
        ClaimId = claimId;
        OrganisationId = organisationId;
        From = from;
        To = to;
        Reason = reason;
        Note = note;
    }

    public Guid ClaimId { get; private set; } = Guid.Empty;

    public ClaimState From { get; private set; }

    public ClaimState To { get; private set; }

    public StateChangeReason Reason { get; private set; }

    public string Note { get; private set; } = string.Empty;
}
