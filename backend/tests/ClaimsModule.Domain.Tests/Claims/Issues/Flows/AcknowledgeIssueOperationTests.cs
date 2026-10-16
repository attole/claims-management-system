using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Flows;
using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Issues.Rules;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Flows;

public sealed class AcknowledgeIssueOperationTests
{
    [Fact]
    public void Run_ResolvesTheIssueAndRecordsWhoAndWhen()
    {
        var claim = TestClaims.WithoutPolicy();
        var issue = claim.Issues.Single(issue => issue.Finding.HasCode(HasLinkedPolicy.Code));
        var input = new IssueResolutionInput(TestClaims.Actor, TestClaims.Later);

        var outcome = AcknowledgeIssueOperation.Run(issue, input);

        outcome.IsSuccess.Should().BeTrue();
        issue.State.Should().BeOfType<IssueAcknowledgedState>();
        issue.ResolvedBy.Should().Be(TestClaims.Actor);
        issue.ResolvedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    [Trait("Rule", IsIssueAcknowledgeable.Code)]
    public void Run_ForTheWrongKindOfIssue_ReturnsTheFinding()
    {
        var claim = TestClaims.WithoutPolicy();
        var issue = claim.Issues.Single(issue => issue.Finding.HasCode(IsLossDatePresent.Code));
        var input = new IssueResolutionInput(TestClaims.Actor, TestClaims.Later);

        var outcome = AcknowledgeIssueOperation.Run(issue, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsIssueAcknowledgeable.Code);
        issue.State.Should().BeOfType<IssueRaisedState>();
    }
}
