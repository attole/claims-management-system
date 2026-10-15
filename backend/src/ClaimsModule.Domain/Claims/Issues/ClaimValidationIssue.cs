using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;

namespace ClaimsModule.Domain.Claims.Issues;

public sealed class ClaimValidationIssue : Entity, IAuditable, ISoftDeletable, ITenantOwned
{
    private ClaimValidationIssue()
    {
        Finding = IssueFinding.Empty;
        State = new IssueRaisedState();
    }

    internal ClaimValidationIssue(Guid id, Guid claimId, Guid organisationId, IssueFinding finding, DateTimeOffset now)
        : base(id)
    {
        ClaimId = claimId;
        OrganisationId = organisationId;
        Finding = finding;
        State = new IssueRaisedState();
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid ClaimId { get; private set; } = Guid.Empty;

    public IssueFinding Finding { get; private set; }

    public IssueState State { get; private set; }

    public DateTimeOffset ResolvedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid ResolvedBy { get; private set; } = Guid.Empty;

    public bool IsCurrent => State.Marker != IssueStateKind.Cleared;

    public bool IsUnresolvedCritical => !State.IsResolved && Finding.Severity.IsBlockingOpening;

    public bool IsUnresolvedWarning => !State.IsResolved && !Finding.Severity.IsBlockingOpening;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid UserCreated { get; private set; } = Guid.Empty;

    public Guid UserModified { get; private set; } = Guid.Empty;

    public bool IsDeleted { get; private set; } = false;

    public DateTimeOffset DeletedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid OrganisationId { get; private set; } = Guid.Empty;

    internal void Clear(Guid actorId, DateTimeOffset now)
    {
        State = new IssueClearedState();
        ResolvedAt = now;
        ResolvedBy = actorId;
        UpdatedAt = now;
    }

    internal void Acknowledge(Guid actorId, DateTimeOffset now)
    {
        State = new IssueAcknowledgedState();
        ResolvedAt = now;
        ResolvedBy = actorId;
        UpdatedAt = now;
    }

    internal void Waive(Guid actorId, DateTimeOffset now)
    {
        State = new IssueWaivedState();
        ResolvedAt = now;
        ResolvedBy = actorId;
        UpdatedAt = now;
    }
}
