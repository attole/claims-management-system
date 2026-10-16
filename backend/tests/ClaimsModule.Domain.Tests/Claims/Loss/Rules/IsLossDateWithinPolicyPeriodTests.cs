using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Rules;

public sealed class IsLossDateWithinPolicyPeriodTests
{
    [Fact]
    [Trait("Rule", IsLossDateWithinPolicyPeriod.Code)]
    public void Check_ForALossInsideThePeriod_Passes()
    {
        var loss = TestClaims.Loss(TestClaims.Start, TestClaims.Description);
        var rule = new IsLossDateWithinPolicyPeriod(TestClaims.LossReference().PolicyPeriod);

        var result = rule.Check(loss);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsLossDateWithinPolicyPeriod.Code)]
    public void Check_ForALossBeforeThePeriod_ReturnsAWarningWithTheDayAndThePeriod()
    {
        var loss = TestClaims.Loss(TestClaims.OutsidePolicyPeriod, TestClaims.Description);
        var rule = new IsLossDateWithinPolicyPeriod(TestClaims.LossReference().PolicyPeriod);

        var result = rule.Check(loss);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(IsLossDateWithinPolicyPeriod.Code);
        finding.Severity.Should().Be(FindingSeverity.Warning);
        finding.Evidence.Should().Be("2025-12-31|2026-01-01|2026-12-31");
    }
}
