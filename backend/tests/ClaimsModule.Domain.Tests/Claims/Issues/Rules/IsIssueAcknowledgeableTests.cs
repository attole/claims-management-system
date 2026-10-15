using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Claims.Issues.Rules;
using ClaimsModule.Domain.Claims.Loss.Checks;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Rules;

public sealed class IsIssueAcknowledgeableTests
{
    [Fact]
    [Trait("Rule", IsIssueAcknowledgeable.Code)]
    public void Check_ForAnUnresolvedWarning_Passes()
    {
        var claim = TestClaims.WithPolicyWarning();
        var issue = claim.Issues.Single(candidate => candidate.Finding.Code == HasLinkedPolicy.Code);
        var rule = new IsIssueAcknowledgeable(issue.Id);

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsIssueAcknowledgeable.Code)]
    public void Check_ForACriticalIssue_ReturnsTheFinding()
    {
        var claim = TestClaims.WithPolicyWarning();
        var issue = claim.Issues.Single(candidate => candidate.Finding.Code == IsLossDatePresent.Code);
        var rule = new IsIssueAcknowledgeable(issue.Id);

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsIssueAcknowledgeable.Code);
    }

    [Fact]
    [Trait("Rule", IsIssueAcknowledgeable.Code)]
    public void Check_ForAnAlreadyAcknowledgedWarning_ReturnsTheFinding()
    {
        var claim = TestClaims.WithPolicyWarning();
        TestClaims.AcknowledgeIssue(claim, HasLinkedPolicy.Code);
        var issue = claim.Issues.Single(candidate => candidate.Finding.Code == HasLinkedPolicy.Code);
        var rule = new IsIssueAcknowledgeable(issue.Id);

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsIssueAcknowledgeable.Code);
    }

    [Fact]
    [Trait("Rule", IsIssueAcknowledgeable.Code)]
    public void Check_ForAnUnknownIssue_ReturnsTheFinding()
    {
        var claim = TestClaims.WithPolicyWarning();
        var rule = new IsIssueAcknowledgeable(Guid.NewGuid());

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsIssueAcknowledgeable.Code);
    }
}
