using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Rules;

public sealed class HasNonNegativeBalanceTests
{
    [Fact]
    [Trait("Rule", HasNonNegativeBalance.Code)]
    public void Check_WhenTheBalanceStaysAtOrAboveZero_Passes()
    {
        var component = TestReserves.Component();
        var rule = new HasNonNegativeBalance(0m);

        var result = rule.Check(component);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNonNegativeBalance.Code)]
    public void Check_WhenTheBalanceWouldGoBelowZero_ReturnsTheFinding()
    {
        var component = TestReserves.Component();
        var rule = new HasNonNegativeBalance(-1m);

        var result = rule.Check(component);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasNonNegativeBalance.Code);
    }
}
