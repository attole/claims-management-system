using ClaimsModule.Domain.CauseOfLoss;

namespace ClaimsModule.Domain.Tests.CauseOfLoss;

public sealed class CauseCodesTests
{
    [Theory]
    [Trait("Rule", "S:BR-C-05")]
    [InlineData("WATER", true)]
    [InlineData("water", true)]
    [InlineData("FIRE", false)]
    [InlineData("", false)]
    public void IsActive_ComparesWithoutCase(string code, bool isExpected)
    {
        var causeCodes = new CauseCodes(["WATER"]);

        var result = causeCodes.IsActive(code);

        result.Should().Be(isExpected);
    }

    [Fact]
    public void CauseCodes_WithoutActiveCodes_Throws()
    {
        Func<CauseCodes> act = () => new CauseCodes([]);

        act.Should().Throw<ArgumentException>();
    }
}
