using ClaimsModule.Domain.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents;

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
    [Trait("Rule", "S:V-ReserveComponent-Type")]
    public void ComponentTypes_CoverEveryKind()
    {
        _pairs.Select(pair => pair.Kind).Should().BeEquivalentTo(Enum.GetValues<ReserveComponentTypeKind>());
    }

    [Fact]
    public void ComponentTypes_CarryTheirOwnMarkerAndName()
    {
        _pairs.Should().OnlyContain(pair => pair.Type.Marker == pair.Kind && pair.Type.Name == pair.Kind.ToString());
    }

    [Theory]
    [Trait("Rule", "S:BR-R-01")]
    [InlineData(0, true)]
    [InlineData(-1, false)]
    public void CanHoldBalance_ForTheIndemnityType_RefusesBelowZero(int balance, bool isExpected)
    {
        var type = new ReserveIndemnityComponentType();

        var result = type.CanHoldBalance(balance);

        result.Should().Be(isExpected);
    }

    [Fact]
    [Trait("Rule", "S:RES-Components")]
    public void CanHoldBalance_ForTheRecoverableType_AllowsBelowZero()
    {
        var type = new ReserveSubrogationRecoverableComponentType();

        var result = type.CanHoldBalance(-5_000m);

        result.Should().BeTrue();
    }
}
