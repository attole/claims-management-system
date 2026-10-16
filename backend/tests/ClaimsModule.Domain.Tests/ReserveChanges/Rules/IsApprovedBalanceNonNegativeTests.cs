using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Rules;

public sealed class IsApprovedBalanceNonNegativeTests
{
    [Fact]
    [Trait("Rule", IsApprovedBalanceNonNegative.Code)]
    public void Check_WhenTheBalanceStaysAtOrAboveZero_Passes()
    {
        var submit = TestReserves.SubmitInput(-50_000m) with { ReserveApprovedBalance = 100_000m };
        var change = TestReserves.Submit(submit);
        var rule = new IsApprovedBalanceNonNegative(50_000m);

        var result = rule.Check(change);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsApprovedBalanceNonNegative.Code)]
    public void Check_WhenTheBalanceHasFallenBelowTheDecrease_ReturnsTheFinding()
    {
        var submit = TestReserves.SubmitInput(-50_000m) with { ReserveApprovedBalance = 100_000m };
        var change = TestReserves.Submit(submit);
        var rule = new IsApprovedBalanceNonNegative(10_000m);

        var result = rule.Check(change);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsApprovedBalanceNonNegative.Code);
    }
}
