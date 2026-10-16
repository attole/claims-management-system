using ClaimsModule.Domain.Claims.Issues.Rules;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Rules;

public sealed class IsIssueAcknowledgeableTests
{
    [Fact]
    [Trait("Rule", IsIssueAcknowledgeable.Code)]
    public void Check_ForAnUnresolvedWarning_Passes()
    {
        var claim = TestClaims.WithoutPolicy();
        var warning = claim.Issues.Single(issue => issue.Finding.HasCode(HasLinkedPolicy.Code));

        var result = new IsIssueAcknowledgeable().Check(warning);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsIssueAcknowledgeable.Code)]
    public void Check_ForACriticalIssue_ReturnsTheFinding()
    {
        var claim = TestClaims.WithoutPolicy();
        var critical = claim.Issues.Single(issue => issue.Finding.HasCode(IsLossDatePresent.Code));

        var result = new IsIssueAcknowledgeable().Check(critical);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsIssueAcknowledgeable.Code);
    }
}
