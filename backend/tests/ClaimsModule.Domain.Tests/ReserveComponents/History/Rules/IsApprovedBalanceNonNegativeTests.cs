using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Rules;

public sealed class IsApprovedBalanceNonNegativeTests
{
    [Fact]
    [Trait("Rule", IsApprovedBalanceNonNegative.Code)]
    public void Check_WhenTheBalanceStaysAtOrAboveZero_Passes()
    {
        var component = TestReserves.ComponentWithApprovedBalance(100_000m);
        var submit = TestReserves.SubmitInput(-50_000m);
        var history = TestReserves.Submit(component, submit);
        var rule = new IsApprovedBalanceNonNegative(50_000m);

        var result = rule.Check(history);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsApprovedBalanceNonNegative.Code)]
    public void Check_WhenTheBalanceHasFallenBelowTheDecrease_ReturnsTheFinding()
    {
        var component = TestReserves.ComponentWithApprovedBalance(100_000m);
        var submit = TestReserves.SubmitInput(-50_000m);
        var history = TestReserves.Submit(component, submit);
        var rule = new IsApprovedBalanceNonNegative(10_000m);

        var result = rule.Check(history);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsApprovedBalanceNonNegative.Code);
    }
}
