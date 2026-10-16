using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Flows;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Flows;

public sealed class ClearIssueOperationTests
{
    [Fact]
    public void Run_ClearsTheIssueWithoutRecordingWhoOrWhen()
    {
        var claim = TestClaims.Empty();
        var issue = claim.Issues[0];

        ClearIssueOperation.Run(issue, NoInput.Value);

        issue.State.Should().BeOfType<IssueClearedState>();
        issue.ResolvedBy.Should().Be(Guid.Empty);
        issue.ResolvedAt.Should().Be(DateTimeOffset.MinValue);
    }

    [Fact]
    public void Run_OnAWaivedIssue_KeepsWhoWaivedItAndWhen()
    {
        var claim = TestClaims.Empty();
        var issue = claim.Issues.First(issue => issue.Finding.HasCode(IsDescriptionLongEnough.Code));
        TestClaims.WaiveIssue(claim, issue.Finding.Code);

        ClearIssueOperation.Run(issue, NoInput.Value);

        issue.ResolvedBy.Should().Be(TestClaims.Actor);
        issue.ResolvedAt.Should().Be(TestClaims.Start);
    }
}
