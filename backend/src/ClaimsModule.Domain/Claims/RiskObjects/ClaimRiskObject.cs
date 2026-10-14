using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;

namespace ClaimsModule.Domain.Claims.RiskObjects;

public sealed class ClaimRiskObject : Entity, IAuditable, ISoftDeletable, ITenantOwned
{
    private ClaimRiskObject()
    {
        Details = new RiskObjectDetails(AssetType.Other, string.Empty);
    }

    internal ClaimRiskObject(Guid id, Guid claimId, Guid organisationId, RiskObjectDetails details, DateTimeOffset now)
        : base(id)
    {
        ClaimId = claimId;
        OrganisationId = organisationId;
        Details = details;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid ClaimId { get; private set; } = Guid.Empty;

    public RiskObjectDetails Details { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid UserCreated { get; private set; } = Guid.Empty;

    public Guid UserModified { get; private set; } = Guid.Empty;

    public bool IsDeleted { get; private set; } = false;

    public DateTimeOffset DeletedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid OrganisationId { get; private set; } = Guid.Empty;

    internal void Remove(DateTimeOffset now)
    {
        IsDeleted = true;
        DeletedAt = now;
        UpdatedAt = now;
    }
}
