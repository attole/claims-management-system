using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.Reserves.Root;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Rules;

public sealed class IsApprovedBalanceNonNegativeTests
{
    [Fact]
    [Trait("Rule", IsApprovedBalanceNonNegative.Code)]
    public void Check_WhenTheBalanceStaysAtOrAboveZero_Passes()
    {
        var submitSnapshot = TestReserves.Snapshot() with { ReserveApprovedBalance = 100_000m };
        var reserve = TestReserves.Submit(-50_000m, submitSnapshot);
        var snapshot = TestReserves.Snapshot() with { ReserveApprovedBalance = 50_000m };
        var rule = new IsApprovedBalanceNonNegative(snapshot);

        var result = rule.Check(reserve);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsApprovedBalanceNonNegative.Code)]
    public void Check_WhenTheBalanceHasFallenBelowTheDecrease_ReturnsTheFinding()
    {
        var submitSnapshot = TestReserves.Snapshot() with { ReserveApprovedBalance = 100_000m };
        var reserve = TestReserves.Submit(-50_000m, submitSnapshot);
        var snapshot = TestReserves.Snapshot() with { ReserveApprovedBalance = 10_000m };
        var rule = new IsApprovedBalanceNonNegative(snapshot);

        var result = rule.Check(reserve);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsApprovedBalanceNonNegative.Code);
    }

    [Fact]
    [Trait("Rule", IsApprovedBalanceNonNegative.Code)]
    public void Check_ForTheRecoverableType_PassesBelowZero()
    {
        var type = new ReserveSubrogationRecoverableComponentType();
        var submitSnapshot = TestReserves.Snapshot() with { ComponentType = type };
        var reserve = TestReserves.Submit(-50_000m, submitSnapshot);
        var rule = new IsApprovedBalanceNonNegative(submitSnapshot);

        var result = rule.Check(reserve);

        result.Findings.Should().BeEmpty();
    }
}
