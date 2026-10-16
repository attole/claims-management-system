using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveChanges.Rules;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Rules;

public sealed class IsReserveAmountNonZeroTests
{
    [Theory]
    [Trait("Rule", IsReserveAmountNonZero.Code)]
    [InlineData(1)]
    [InlineData(-1)]
    public void Check_WithANonZeroAmount_Passes(int amount)
    {
        var rule = new IsReserveAmountNonZero(amount);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsReserveAmountNonZero.Code)]
    public void Check_WithAZeroAmount_ReturnsTheFinding()
    {
        var rule = new IsReserveAmountNonZero(0m);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsReserveAmountNonZero.Code);
    }
}
