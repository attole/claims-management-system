using ClaimsModule.Domain.Claims.Parties.Events;
using ClaimsModule.Domain.Claims.Parties.Flows;
using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Parties;

public sealed class ClaimParty : Entity, ISoftDeletable
{
    private ClaimParty()
    {
        Details = new PartyDetails(new PartyWitnessRole(), new PartyPersonType());
    }

    internal ClaimParty(Guid id, Guid claimId, Guid organisationId, PartyDetails details) : base(id)
    {
        ClaimId = claimId;
        OrganisationId = organisationId;
        Details = details;
        IsActive = true;
    }

    public Guid ClaimId { get; private set; } = Guid.Empty;

    public PartyDetails Details { get; private set; }

    public bool IsActive { get; internal set; } = false;

    public bool IsActiveClaimant => IsActive && Details.Role.IsClaimant;

    public bool IsDeleted { get; private set; } = false;

    internal Outcome<ClaimParty> Remove()
    {
        return RemovePartyOperation.Run(this, NoInput.Value)
            .OnSuccess(party => Raise(new PartyRemovedEvent(ClaimId, party.Id, party.Details.Role.Name)));
    }
}
