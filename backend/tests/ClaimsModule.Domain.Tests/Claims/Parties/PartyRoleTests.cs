using ClaimsModule.Domain.Claims.Parties;

namespace ClaimsModule.Domain.Tests.Claims.Parties;

public sealed class PartyRoleTests
{
    private static readonly PartyRole[] _roles =
    [
        new PartyClaimantRole(),
        new PartyInsuredRole(),
        new PartyThirdPartyRole(),
        new PartyWitnessRole(),
        new PartyAttorneyRole(),
    ];

    [Fact]
    [Trait("Rule", "S:BR-P-02")]
    public void Roles_OnlyTheClaimantIsAClaimant()
    {
        _roles.Where(role => role.IsClaimant).Should().ContainSingle().Which.Should().Be(new PartyClaimantRole());
    }

    [Fact]
    [Trait("Rule", "S:BR-P-02")]
    public void Roles_CarryTheirOwnMarkerAndName()
    {
        _roles.Select(role => role.Marker).Should().BeEquivalentTo(Enum.GetValues<PartyRoleKind>());
        _roles.Should().OnlyContain(role => role.Name == role.Marker.ToString());
    }
}
