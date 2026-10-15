using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Checks;
using ClaimsModule.Domain.Claims.Loss.Flows;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Flows;

public sealed class EvaluateLossFlowTests
{
    [Fact]
    public void Execute_ForACompleteLoss_ReportsNothing()
    {
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-1), TestClaims.Description);
        var input = new LossEvaluationInput(TestClaims.Later, TestClaims.Start, TestClaims.Reference());

        var outcome = EvaluateLossFlow.Execute(loss, input);

        outcome.Result.HasNoFindings.Should().BeTrue();
    }

    [Fact]
    public void Execute_ForAnEmptyLoss_ReportsEveryMissingValue()
    {
        var input = new LossEvaluationInput(TestClaims.Later, TestClaims.Start, TestClaims.Reference());

        var outcome = EvaluateLossFlow.Execute(LossDetails.Empty, input);

        outcome.Blocking.Select(finding => finding.Code).Should().BeEquivalentTo(
            IsLossDatePresent.Code,
            IsDescriptionLongEnough.Code,
            IsCauseCodeActive.Code);
    }

    [Fact]
    public void Execute_ForALossAfterTheReportAndNow_ReportsBothDateProblems()
    {
        var loss = TestClaims.Loss(TestClaims.Later.AddDays(1), TestClaims.Description);
        var input = new LossEvaluationInput(TestClaims.Later, TestClaims.Start, TestClaims.Reference());

        var outcome = EvaluateLossFlow.Execute(loss, input);

        outcome.Blocking.Select(finding => finding.Code).Should().BeEquivalentTo(
            IsLossDateNotInFuture.Code,
            IsLossDateNotAfterReport.Code);
    }

    [Fact]
    public void Execute_ForALossOutsideThePolicyPeriod_ReportsAnAdvisoryFinding()
    {
        var loss = TestClaims.Loss(TestClaims.OutsidePolicyPeriod, TestClaims.Description);
        var input = new LossEvaluationInput(TestClaims.Later, TestClaims.Start, TestClaims.Reference());

        var outcome = EvaluateLossFlow.Execute(loss, input);

        outcome.Advisory.Should().ContainSingle().Which.Code.Should().Be(IsLossDateWithinPolicyPeriod.Code);
    }
}
