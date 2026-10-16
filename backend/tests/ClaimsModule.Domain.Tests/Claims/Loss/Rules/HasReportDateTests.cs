using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Rules;

public sealed class HasReportDateTests
{
    [Fact]
    [Trait("Rule", HasReportDate.Code)]
    public void Check_WithAReportDate_Passes()
    {
        var rule = new HasReportDate(TestClaims.Start);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasReportDate.Code)]
    public void Check_WithoutAReportDate_ReturnsTheFinding()
    {
        var rule = new HasReportDate(DateTimeOffset.MinValue);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasReportDate.Code);
    }
}
