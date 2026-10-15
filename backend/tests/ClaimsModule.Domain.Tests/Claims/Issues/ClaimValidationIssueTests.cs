using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Claims.Issues.Operations;
using ClaimsModule.Domain.Claims.Loss.Operations;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues;

public sealed class ClaimValidationIssueTests
{
    private static readonly Guid _otherActor = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void ARaisedCriticalIssue_IsCurrentAndUnresolved()
    {
        var claim = TestClaims.Empty();

        claim.Issues.Should().OnlyContain(issue => issue.IsCurrent && issue.IsUnresolvedCritical);
        claim.Issues.Should().OnlyContain(issue => issue.State is IssueRaisedState);
    }

    [Fact]
    public void AFixedProblem_ClearsTheIssueAndRecordsTheTimeAndTheActor()
    {
        var claim = TestClaims.Empty();
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-1), TestClaims.Description);
        var update = new UpdateLossDetailsInput(loss, TestClaims.Actor, TestClaims.Later);
        new UpdateLossDetailsOperation().Execute(claim, update);
        var input = new EvaluateIssuesInput(TestClaims.Later, TestClaims.Actor, TestClaims.Reference());

        new EvaluateIssuesFlow().Execute(claim, input);

        var dateIssue = claim.Issues.Single(issue => issue.Finding.Code == IsLossDatePresent.Code);
        dateIssue.State.Should().BeOfType<IssueClearedState>();
        dateIssue.IsCurrent.Should().BeFalse();
        dateIssue.ResolvedAt.Should().Be(TestClaims.Later);
        dateIssue.ResolvedBy.Should().Be(TestClaims.Actor);
    }

    [Fact]
    public void AWaivedIssue_IsResolvedButStillCurrent()
    {
        var claim = TestClaims.Empty();
        var issue = claim.Issues[0];
        var input = new WaiveIssueInput(issue.Id, TestClaims.Actor, TestClaims.Later);

        new WaiveIssueOperation().Execute(claim, input);

        issue.IsCurrent.Should().BeTrue();
        issue.IsUnresolvedCritical.Should().BeFalse();
        issue.ResolvedBy.Should().Be(TestClaims.Actor);
    }

    [Fact]
    public void AWaivedIssue_WhoseProblemIsFixed_IsClearedByTheActorWhoFixedIt()
    {
        var claim = TestClaims.Empty();
        var issue = claim.Issues.Single(candidate => candidate.Finding.Code == IsLossDatePresent.Code);
        var waive = new WaiveIssueInput(issue.Id, TestClaims.Actor, TestClaims.Later);
        new WaiveIssueOperation().Execute(claim, waive);
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-1), TestClaims.Description);
        var fix = new UpdateLossDetailsInput(loss, _otherActor, TestClaims.Later.AddHours(1));
        new UpdateLossDetailsOperation().Execute(claim, fix);
        var input = new EvaluateIssuesInput(TestClaims.Later.AddHours(1), _otherActor, TestClaims.Reference());

        new EvaluateIssuesFlow().Execute(claim, input);

        issue.State.Should().BeOfType<IssueClearedState>();
        issue.ResolvedBy.Should().Be(_otherActor);
        issue.ResolvedAt.Should().Be(TestClaims.Later.AddHours(1));
    }
}
