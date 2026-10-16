using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class IsOverrideRevocableTests
{
    [Theory]
    [Trait("Rule", IsOverrideRevocable.Code)]
    [InlineData(true, 12_000_000)]
    [InlineData(false, 10_000_000)]
    public void Check_WhenSettingItOrWithinTheLimit_Passes(bool hasAggregateLimitOverride, int claimApprovedTotal)
    {
        var rule = new IsOverrideRevocable(hasAggregateLimitOverride, claimApprovedTotal);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsOverrideRevocable.Code)]
    public void Check_WhenRemovingItAboveTheLimit_ReturnsTheFinding()
    {
        var rule = new IsOverrideRevocable(false, 10_000_001m);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsOverrideRevocable.Code);
    }
}
