using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.ReserveComponents.History.Rules;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Rules;

public sealed class IsClaimTotalWithinLimitTests
{
    [Fact]
    [Trait("Rule", IsClaimTotalWithinLimit.Code)]
    public void Check_WhenTheTotalStaysWithinTheLimit_Passes()
    {
        var rule = new IsClaimTotalWithinLimit(9_990_000m, 10_000m);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsClaimTotalWithinLimit.Code)]
    public void Check_WhenTheTotalPassesTheLimit_ReturnsAWarning()
    {
        var rule = new IsClaimTotalWithinLimit(9_990_000m, 10_001m);

        var result = rule.Check(NoSubject.Value);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(IsClaimTotalWithinLimit.Code);
        finding.Severity.Should().Be(FindingSeverity.Warning);
    }

    [Fact]
    [Trait("Rule", "S:V-Messages")]
    public void Check_WhenTheTotalPassesTheLimit_ReturnsTheSpecMessage()
    {
        var rule = new IsClaimTotalWithinLimit(9_990_000m, 10_001m);

        var result = rule.Check(NoSubject.Value);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Message.Should().Be("Total reserves will exceed $10,000,000. Manager override required.");
    }
}
