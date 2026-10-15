using ClaimsModule.Domain.Claims.Loss.Checks;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Checks;

public sealed class IsLossDateNotAfterReportTests
{
    [Fact]
    [Trait("Rule", IsLossDateNotAfterReport.Code)]
    public void Check_FailsWhenTheLossDateIsAfterTheReportDate()
    {
        var claim = TestClaims.WithLoss(TestClaims.Start.AddTicks(1));

        var result = new IsLossDateNotAfterReport(TestClaims.Start).Check(claim.LossEvent.Details);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(IsLossDateNotAfterReport.Code);
        finding.IsBlocking.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", IsLossDateNotAfterReport.Code)]
    public void Check_PassesWhenTheLossDateIsTheReportDate()
    {
        var claim = TestClaims.WithLoss(TestClaims.Start);

        var result = new IsLossDateNotAfterReport(TestClaims.Start).Check(claim.LossEvent.Details);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsLossDateNotAfterReport.Code)]
    public void Check_PassesWithoutALossDate()
    {
        var claim = TestClaims.Empty();

        var result = new IsLossDateNotAfterReport(TestClaims.Start).Check(claim.LossEvent.Details);

        result.Findings.Should().BeEmpty();
    }
}
