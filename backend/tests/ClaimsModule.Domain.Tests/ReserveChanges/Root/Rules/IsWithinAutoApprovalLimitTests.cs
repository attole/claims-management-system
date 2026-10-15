using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Rules;

public sealed class IsWithinAutoApprovalLimitTests
{
    [Theory]
    [Trait("Rule", IsWithinAutoApprovalLimit.Code)]
    [Trait("Rule", "S:RES-Threshold-PerTransaction")]
    [InlineData(1)]
    [InlineData(10_000)]
    [InlineData(-10_000)]
    public void Check_AtOrBelowTheAutoLimit_Passes(int amount)
    {
        var snapshot = TestReserves.Snapshot() with { ReserveApprovedBalance = 100_000m };
        var reserve = TestReserves.Submit(amount, snapshot);
        var rule = new IsWithinAutoApprovalLimit();

        var result = rule.Check(reserve);

        result.Findings.Should().BeEmpty();
    }

    [Theory]
    [Trait("Rule", IsWithinAutoApprovalLimit.Code)]
    [Trait("Rule", "S:RES-Threshold-PerTransaction")]
    [InlineData(10_001)]
    [InlineData(-50_000)]
    public void Check_AboveTheAutoLimit_ReturnsTheFinding(int amount)
    {
        var snapshot = TestReserves.Snapshot() with { ReserveApprovedBalance = 100_000m };
        var reserve = TestReserves.Submit(amount, snapshot);
        var rule = new IsWithinAutoApprovalLimit();

        var result = rule.Check(reserve);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsWithinAutoApprovalLimit.Code);
    }
}
