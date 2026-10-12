using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.Claims.Parties;

public enum PartyTypeKind
{
    Person,
    Company,
}

public abstract record PartyType(PartyTypeKind Marker) : Marked<PartyTypeKind>(Marker)
{
    public abstract bool UsesPersonName { get; }

    public bool UsesCompanyName => !UsesPersonName;
}

public sealed record PersonPartyType() : PartyType(PartyTypeKind.Person)
{
    public override bool UsesPersonName => true;
}

public sealed record CompanyPartyType() : PartyType(PartyTypeKind.Company)
{
    public override bool UsesPersonName => false;
}
