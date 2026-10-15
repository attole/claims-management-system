using ClaimsModule.Domain.Claims.Issues.Flows;
using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Issues.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Rules;

public sealed class IsIssueWaivableTests
{
    [Fact]
    [Trait("Rule", IsIssueWaivable.Code)]
    public void Check_PassesForAnUnresolvedCriticalIssue()
    {
        var claim = TestClaims.Empty();

        var result = new IsIssueWaivable(claim.Issues[0].Id).Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsIssueWaivable.Code)]
    public void Check_FailsForAnUnknownIssue()
    {
        var claim = TestClaims.Empty();

        var result = new IsIssueWaivable(Guid.NewGuid()).Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsIssueWaivable.Code);
    }

    [Fact]
    [Trait("Rule", IsIssueWaivable.Code)]
    public void Check_FailsForAnIssueThatIsAlreadyResolved()
    {
        var claim = TestClaims.Empty();
        var issueId = claim.Issues[0].Id;
        var input = new WaiveIssueInput(issueId, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());
        WaiveIssueOperation.Execute(claim, input);

        var result = new IsIssueWaivable(issueId).Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsIssueWaivable.Code);
    }
}
