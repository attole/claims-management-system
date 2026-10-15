using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Rules;

public sealed class IsClaimLimitClearedTests
{
    [Fact]
    [Trait("Rule", IsClaimLimitCleared.Code)]
    public void Check_WhenTheTotalStaysWithinTheLimit_Passes()
    {
        var snapshot = TestReserves.Snapshot() with { ClaimApprovedTotal = 1_000_000m };
        var reserve = TestReserves.Submit(50_000m);
        var rule = new IsClaimLimitCleared(snapshot);

        var result = rule.Check(reserve);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsClaimLimitCleared.Code)]
    public void Check_WhenTheTotalPassesTheLimitWithoutAnOverride_ReturnsTheFinding()
    {
        var snapshot = TestReserves.Snapshot() with { ClaimApprovedTotal = 9_990_000m };
        var reserve = TestReserves.Submit(50_000m);
        var rule = new IsClaimLimitCleared(snapshot);

        var result = rule.Check(reserve);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(IsClaimLimitCleared.Code);
        finding.IsBlocking.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", IsClaimLimitCleared.Code)]
    public void Check_WhenTheTotalPassesTheLimitWithAnOverride_Passes()
    {
        var snapshot = TestReserves.Snapshot() with { ClaimApprovedTotal = 9_990_000m, HasLimitOverride = true };
        var reserve = TestReserves.Submit(50_000m);
        var rule = new IsClaimLimitCleared(snapshot);

        var result = rule.Check(reserve);

        result.Findings.Should().BeEmpty();
    }
}
