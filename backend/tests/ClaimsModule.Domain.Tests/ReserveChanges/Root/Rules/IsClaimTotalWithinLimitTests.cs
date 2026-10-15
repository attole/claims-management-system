using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Rules;

public sealed class IsClaimTotalWithinLimitTests
{
    [Fact]
    [Trait("Rule", IsClaimTotalWithinLimit.Code)]
    public void Check_WhenTheTotalStaysWithinTheLimit_Passes()
    {
        var snapshot = TestReserves.Snapshot() with { ClaimApprovedTotal = 9_990_000m };
        var rule = new IsClaimTotalWithinLimit(snapshot, 10_000m);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsClaimTotalWithinLimit.Code)]
    public void Check_WhenTheTotalPassesTheLimit_ReturnsAnAdvisoryFinding()
    {
        var snapshot = TestReserves.Snapshot() with { ClaimApprovedTotal = 9_990_000m };
        var rule = new IsClaimTotalWithinLimit(snapshot, 10_001m);

        var result = rule.Check(NoSubject.Value);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(IsClaimTotalWithinLimit.Code);
        finding.IsBlocking.Should().BeFalse();
    }
}
