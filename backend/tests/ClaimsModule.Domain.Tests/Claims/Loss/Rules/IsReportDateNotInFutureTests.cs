using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Rules;

public sealed class IsReportDateNotInFutureTests
{
    [Theory]
    [Trait("Rule", IsReportDateNotInFuture.Code)]
    [InlineData(0)]
    [InlineData(-48)]
    public void Check_AtOrBeforeTheCreationTime_Passes(int hoursFromNow)
    {
        var reportDate = TestClaims.Start.AddHours(hoursFromNow);
        var rule = new IsReportDateNotInFuture(reportDate, TestClaims.Start);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsReportDateNotInFuture.Code)]
    public void Check_AfterTheCreationTime_ReturnsTheFinding()
    {
        var reportDate = TestClaims.Start.AddMinutes(1);
        var rule = new IsReportDateNotInFuture(reportDate, TestClaims.Start);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsReportDateNotInFuture.Code);
    }
}
