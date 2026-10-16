using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Rules;

public sealed class IsClaimLimitClearedTests
{
    [Fact]
    [Trait("Rule", IsClaimLimitCleared.Code)]
    public void Check_WhenTheTotalStaysWithinTheLimit_Passes()
    {
        var change = TestReserves.Submit(50_000m);
        var rule = new IsClaimLimitCleared(1_000_000m, false);

        var result = rule.Check(change);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsClaimLimitCleared.Code)]
    public void Check_WhenTheTotalPassesTheLimitWithoutAnOverride_ReturnsTheFinding()
    {
        var change = TestReserves.Submit(50_000m);
        var rule = new IsClaimLimitCleared(9_990_000m, false);

        var result = rule.Check(change);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(IsClaimLimitCleared.Code);
        finding.Severity.Should().Be(FindingSeverity.Critical);
    }

    [Fact]
    [Trait("Rule", IsClaimLimitCleared.Code)]
    public void Check_WhenTheTotalPassesTheLimitWithAnOverride_Passes()
    {
        var change = TestReserves.Submit(50_000m);
        var rule = new IsClaimLimitCleared(9_990_000m, true);

        var result = rule.Check(change);

        result.Findings.Should().BeEmpty();
    }
}
