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
    [Trait("Rule", IsLossDateNotInFuture.Code)]
    public void Check_ForALossInTheFuture_ReportsOneDateProblem()
    {
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(1), TestClaims.Description);

        var result = CreateRule().Check(loss);

        result.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsLossDateNotInFuture.Code);
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
        return new HasValidLossDetails(TestClaims.Start, TestClaims.Period(), TestClaims.Codes());
    }
}
