using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Rules;

public sealed class HasNoUnresolvedCriticalIssuesTests
{
    [Fact]
    [Trait("Rule", HasNoUnresolvedCriticalIssues.Code)]
    public void Check_WhenNoCriticalIssueIsUnresolved_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasNoUnresolvedCriticalIssues();

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNoUnresolvedCriticalIssues.Code)]
    public void Check_WhenACriticalIssueIsUnresolved_ReturnsTheFinding()
    {
        var claim = TestClaims.WithLoss(TestClaims.Start.AddDays(2));
        var rule = new HasNoUnresolvedCriticalIssues();

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasNoUnresolvedCriticalIssues.Code);
    }
}
