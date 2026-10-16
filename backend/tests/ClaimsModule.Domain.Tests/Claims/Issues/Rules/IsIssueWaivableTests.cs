using ClaimsModule.Domain.Claims.Issues.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Rules;

public sealed class IsIssueWaivableTests
{
    [Fact]
    [Trait("Rule", IsIssueWaivable.Code)]
    public void Check_ForAnUnresolvedCriticalIssue_Passes()
    {
        var claim = TestClaims.Empty();

        var result = new IsIssueWaivable().Check(claim.Issues[0]);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsIssueWaivable.Code)]
    public void Check_ForAWarning_ReturnsTheFinding()
    {
        var claim = TestClaims.WithoutPolicy();
        var warning = claim.Issues.Single(issue => issue.Finding.HasCode(HasLinkedPolicy.Code));

        var result = new IsIssueWaivable().Check(warning);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsIssueWaivable.Code);
    }
}
