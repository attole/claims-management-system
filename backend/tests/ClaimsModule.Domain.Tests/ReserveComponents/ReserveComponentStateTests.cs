using ClaimsModule.Domain.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents;

public sealed class ReserveComponentStateTests
{
    private static readonly (ReserveComponentState State, ReserveComponentStateKind Kind)[] _pairs =
    [
        (new ReserveComponentActiveState(), ReserveComponentStateKind.Active),
        (new ReserveComponentClosedState(), ReserveComponentStateKind.Closed),
    ];

    [Fact]
    public void States_CoverEveryKind()
    {
        _pairs.Select(pair => pair.Kind).Should().BeEquivalentTo(Enum.GetValues<ReserveComponentStateKind>());
    }

    [Fact]
    public void States_CarryTheirOwnMarkerAndName()
    {
        _pairs.Should().OnlyContain(
            pair => pair.State.Marker == pair.Kind && pair.State.Name == pair.Kind.ToString());
    }

    [Fact]
    public void IsActive_IsTrueOnlyForTheActiveState()
    {
        var active = _pairs.Where(pair => pair.State.IsActive).Select(pair => pair.Kind);

        active.Should().BeEquivalentTo([ReserveComponentStateKind.Active]);
    }
}
