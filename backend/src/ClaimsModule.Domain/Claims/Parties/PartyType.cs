using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.Claims.Parties;

public enum PartyTypeKind
{
    Person,
    Company,
}

public abstract record PartyType(PartyTypeKind Marker) : Marked<PartyTypeKind>(Marker)
{
    public abstract bool HasPersonName { get; }

    public bool HasCompanyName => !HasPersonName;

    public static PartyType From(PartyTypeKind kind)
    {
        return ClosedSet<PartyType, PartyTypeKind>.From(kind);
    }
}

public sealed record PartyPersonType() : PartyType(PartyTypeKind.Person)
{
    public override bool HasPersonName => true;
}

public sealed record PartyCompanyType() : PartyType(PartyTypeKind.Company)
{
    public override bool HasPersonName => false;
}
