using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Rules;

public sealed class IsLossDateNotAfterReportTests
{
    [Fact]
    [Trait("Rule", IsLossDateNotAfterReport.Code)]
    public void Check_ForALossLaterOnTheReportDay_Passes()
    {
        var loss = TestClaims.Loss(TestClaims.Start.AddHours(5), TestClaims.Description);

        var result = new IsLossDateNotAfterReport(TestClaims.ReportDay).Check(loss);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsLossDateNotAfterReport.Code)]
    [Trait("Rule", "S:BR-C-01")]
    [Trait("Rule", "A:BR-C-01")]
    public void Check_ForALossOnALaterDay_ReturnsACriticalFindingWithBothDays()
    {
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(1), TestClaims.Description);

        var result = new IsLossDateNotAfterReport(TestClaims.ReportDay).Check(loss);

        var finding = result.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be(IsLossDateNotAfterReport.Code);
        finding.Severity.Should().Be(FindingSeverity.Critical);
        finding.Evidence.Should().Be("2026-10-13|2026-10-12");
    }
}
