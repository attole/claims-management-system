using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;

namespace ClaimsModule.Domain.Claims.StateChanges;

public sealed class ClaimStateChange : Entity, IAuditable, ITenantOwned
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
        DateTimeOffset now)
        : base(id)
    {
        ClaimId = claimId;
        OrganisationId = organisationId;
        From = from;
        To = to;
        Reason = reason;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid ClaimId { get; private set; } = Guid.Empty;

    public ClaimState From { get; private set; }

    public ClaimState To { get; private set; }

    public StateChangeReason Reason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid UserCreated { get; private set; } = Guid.Empty;

    public Guid UserModified { get; private set; } = Guid.Empty;

    public Guid OrganisationId { get; private set; } = Guid.Empty;
}
