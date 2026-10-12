using ClaimsModule.Domain.Claims.Parties;

namespace ClaimsModule.Domain.Tests.Claims.Parties;

public sealed class PartyRoleTests
{
    private static readonly PartyRole[] _roles =
    [
        new ClaimantRole(),
        new InsuredRole(),
        new ThirdPartyRole(),
        new WitnessRole(),
        new AttorneyRole(),
    ];

    [Fact]
    [Trait("Rule", "S:BR-P-02")]
    public void OnlyTheClaimantRoleCountsAsClaimant()
    {
        _roles.Where(role => role.CountsAsClaimant).Should().ContainSingle().Which.Should().Be(new ClaimantRole());
    }

    [Fact]
    [Trait("Rule", "S:BR-P-02")]
    public void EveryRoleCarriesItsOwnMarkerAndName()
    {
        _roles.Select(role => role.Marker).Should().BeEquivalentTo(Enum.GetValues<PartyRoleKind>());
        _roles.Should().OnlyContain(role => role.Name == role.Marker.ToString());
    }
}
