using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.Claims.Parties;

public enum PartyRoleKind
{
    Claimant,
    Insured,
    ThirdParty,
    Witness,
    Attorney,
}

public abstract record PartyRole(PartyRoleKind Marker) : Marked<PartyRoleKind>(Marker)
{
    public virtual bool CountsAsClaimant => false;
}

public sealed record ClaimantRole() : PartyRole(PartyRoleKind.Claimant)
{
    public override bool CountsAsClaimant => true;
}

public sealed record InsuredRole() : PartyRole(PartyRoleKind.Insured);

public sealed record ThirdPartyRole() : PartyRole(PartyRoleKind.ThirdParty);

public sealed record WitnessRole() : PartyRole(PartyRoleKind.Witness);

public sealed record AttorneyRole() : PartyRole(PartyRoleKind.Attorney);
