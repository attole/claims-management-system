using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Rules;

public sealed class HasValidLossDetailsTests
{
    [Fact]
    public void Check_ForACompleteLoss_ReportsNothing()
    {
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-1), TestClaims.Description);

        var result = CreateRule().Check(loss);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    public void Check_ForAnEmptyLoss_ReportsEveryMissingValue()
    {
        var result = CreateRule().Check(LossDetails.Empty);

        result.CriticalFindings.Select(finding => finding.Code).Should().BeEquivalentTo(
            IsLossDatePresent.Code,
            IsDescriptionLongEnough.Code,
            IsCauseCodeActive.Code);
    }

    [Fact]
    [Trait("Rule", "S:BR-C-01")]
    [Trait("Rule", "A:BR-C-01")]
    public void Check_ForALossAfterTheReport_ReportsOneDateProblem()
    {
        var loss = TestClaims.Loss(TestClaims.Later.AddDays(1), TestClaims.Description);

        var result = CreateRule().Check(loss);

        result.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsLossDateNotAfterReport.Code);
    }

    [Fact]
    public void Check_ForALossOutsideThePolicyPeriod_ReportsAWarning()
    {
        var loss = TestClaims.Loss(TestClaims.OutsidePolicyPeriod, TestClaims.Description);

        var result = CreateRule().Check(loss);

        result.WarningFindings.Should().ContainSingle().Which.Code.Should().Be(IsLossDateWithinPolicyPeriod.Code);
    }

    private static HasValidLossDetails CreateRule()
    {
        return new HasValidLossDetails(TestClaims.ReportDay, TestClaims.Period(), TestClaims.Codes());
    }
}
