using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;

namespace ClaimsModule.Domain.Claims.Parties;

public sealed class ClaimParty : Entity, IAuditable, ISoftDeletable, ITenantOwned
{
    private ClaimParty()
    {
        Details = new PartyDetails(new WitnessRole(), new PersonPartyType());
    }

    internal ClaimParty(Guid id, Guid claimId, Guid organisationId, PartyDetails details, DateTimeOffset now)
        : base(id)
    {
        ClaimId = claimId;
        OrganisationId = organisationId;
        Details = details;
        IsActive = true;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid ClaimId { get; private set; }

    public PartyDetails Details { get; private set; }

    public bool IsActive { get; private set; }

    public bool IsActiveClaimant => IsActive && Details.Role.CountsAsClaimant;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public Guid UserCreated { get; private set; } = Guid.Empty;

    public Guid UserModified { get; private set; } = Guid.Empty;

    public bool IsDeleted { get; private set; } = false;

    public DateTimeOffset DeletedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid OrganisationId { get; private set; }

    internal void Deactivate(DateTimeOffset now)
    {
        IsActive = false;
        UpdatedAt = now;
    }
}
