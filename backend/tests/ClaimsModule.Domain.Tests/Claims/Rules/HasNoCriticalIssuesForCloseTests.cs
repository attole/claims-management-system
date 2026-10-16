using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasNoCriticalIssuesForCloseTests
{
    [Fact]
    [Trait("Rule", HasNoCriticalIssuesForClose.Code)]
    public void Check_WhenNoCriticalIssueIsUnresolved_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasNoCriticalIssuesForClose();

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNoCriticalIssuesForClose.Code)]
    public void Check_WhenACriticalIssueIsUnresolved_ReturnsTheFinding()
    {
        var claim = TestClaims.WithLoss(TestClaims.Start.AddDays(2));
        var rule = new HasNoCriticalIssuesForClose();

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasNoCriticalIssuesForClose.Code);
    }
}
