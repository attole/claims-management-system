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
    public virtual bool IsClaimant => false;
}

public sealed record PartyClaimantRole() : PartyRole(PartyRoleKind.Claimant)
{
    public override bool IsClaimant => true;
}

public sealed record PartyInsuredRole() : PartyRole(PartyRoleKind.Insured);

public sealed record PartyThirdPartyRole() : PartyRole(PartyRoleKind.ThirdParty);

public sealed record PartyWitnessRole() : PartyRole(PartyRoleKind.Witness);

public sealed record PartyAttorneyRole() : PartyRole(PartyRoleKind.Attorney);
