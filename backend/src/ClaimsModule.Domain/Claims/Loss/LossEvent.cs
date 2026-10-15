using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;

namespace ClaimsModule.Domain.Claims.Loss;

public sealed class LossEvent : Entity, IAuditable, ISoftDeletable, ITenantOwned
{
    private LossEvent()
    {
        Details = LossDetails.Empty;
    }

    internal LossEvent(Guid id, Guid claimId, Guid organisationId, LossDetails details, DateTimeOffset now) : base(id)
    {
        ClaimId = claimId;
        OrganisationId = organisationId;
        ReportDate = now;
        CreatedAt = now;
        UpdatedAt = now;
        Details = details;
    }

    public Guid ClaimId { get; private set; } = Guid.Empty;

    public LossDetails Details { get; private set; }

    public DateTimeOffset ReportDate { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid UserCreated { get; private set; } = Guid.Empty;

    public Guid UserModified { get; private set; } = Guid.Empty;

    public bool IsDeleted { get; private set; } = false;

    public DateTimeOffset DeletedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid OrganisationId { get; private set; } = Guid.Empty;

    internal void Update(LossDetails details, DateTimeOffset now)
    {
        Details = details;
        UpdatedAt = now;
    }
}
