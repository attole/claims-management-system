using ClaimsModule.Domain.Reserves.Root;

namespace ClaimsModule.Domain.Tests.Reserves.Root;

public sealed class ReserveComponentTypeTests
{
    private static readonly (ReserveComponentType Type, ReserveComponentTypeKind Kind)[] _pairs =
    [
        (new ReserveIndemnityComponentType(), ReserveComponentTypeKind.Indemnity),
        (new ReserveExpenseComponentType(), ReserveComponentTypeKind.Expense),
        (new ReserveAlaeComponentType(), ReserveComponentTypeKind.Alae),
        (new ReserveSubrogationRecoverableComponentType(), ReserveComponentTypeKind.SubrogationRecoverable),
    ];

    [Fact]
    public void ComponentTypes_CoverEveryKind()
    {
        _pairs.Select(pair => pair.Kind).Should().BeEquivalentTo(Enum.GetValues<ReserveComponentTypeKind>());
    }

    [Fact]
    public void ComponentTypes_CarryTheirOwnMarkerAndName()
    {
        _pairs.Should().OnlyContain(pair => pair.Type.Marker == pair.Kind && pair.Type.Name == pair.Kind.ToString());
    }

    [Fact]
    [Trait("Rule", "S:RES-Components")]
    public void CanGoNegative_IsTrueOnlyForTheRecoverableType()
    {
        var negative = _pairs.Where(pair => pair.Type.CanGoNegative).Select(pair => pair.Kind);

        negative.Should().ContainSingle().Which.Should().Be(ReserveComponentTypeKind.SubrogationRecoverable);
    }
}
