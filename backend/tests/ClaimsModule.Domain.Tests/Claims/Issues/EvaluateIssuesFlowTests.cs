using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Claims.Loss.Operations;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues;

public sealed class EvaluateIssuesFlowTests
{
    private static readonly string[] _emptyClaimCodes =
    [
        IsLossDatePresent.Code,
        IsDescriptionLongEnough.Code,
        IsCauseCodeActive.Code,
        HasClaimant.Code,
    ];

    [Fact]
    public void Execute_WhenFindingsBlock_SucceedsAndReportsThem()
    {
        var claim = TestClaims.Empty();
        var input = new EvaluateIssuesInput(TestClaims.Later, TestClaims.Actor, TestClaims.Reference());

        var outcome = new EvaluateIssuesFlow().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Blocking.Should().HaveCount(4);
    }

    [Fact]
    public void Execute_WhenRunTwice_StoresNothingNew()
    {
        var claim = TestClaims.Empty();
        var input = new EvaluateIssuesInput(TestClaims.Later, TestClaims.Actor, TestClaims.Reference());
        var flow = new EvaluateIssuesFlow();
        flow.Execute(claim, input);

        var outcome = flow.Execute(claim, input);

        claim.Issues.Should().HaveCount(4);
        outcome.Value.Raised.Should().BeEmpty();
        outcome.Value.Cleared.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:VAL-Severity")]
    public void ANewClaim_StoresTheCriticalIssuesForWhatIsMissing()
    {
        var claim = TestClaims.Empty();

        claim.Issues.Select(issue => issue.Finding.Code).Should().BeEquivalentTo(_emptyClaimCodes);
        claim.Issues.Should().OnlyContain(issue => issue.Finding.Severity.IsBlockingOpening);
        claim.BlockingIssues.Should().HaveCount(4);
    }

    [Fact]
    public void ACompleteClaim_HasNoBlockingIssueAndEveryEarlierIssueIsCleared()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        claim.BlockingIssues.Should().BeEmpty();
        claim.Issues.Should().OnlyContain(issue => !issue.IsCurrent);
    }

    [Fact]
    public void AFixedProblem_ClearsItsIssue()
    {
        var claim = TestClaims.Empty();
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-1), TestClaims.Description);
        var update = new UpdateLossDetailsInput(loss, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());
        new UpdateLossDetailsOperation().Execute(claim, update);
        var input = new EvaluateIssuesInput(TestClaims.Later, TestClaims.Actor, TestClaims.Reference());

        var outcome = new EvaluateIssuesFlow().Execute(claim, input);

        claim.BlockingIssues.Select(issue => issue.Finding.Code).Should().Equal(HasClaimant.Code);
        string[] clearedCodes = [IsLossDatePresent.Code, IsDescriptionLongEnough.Code, IsCauseCodeActive.Code];
        outcome.Value.Cleared.Select(issue => issue.Finding.Code).Should().BeEquivalentTo(clearedCodes);
        outcome.Value.Raised.Should().BeEmpty();
    }

    [Fact]
    public void ANewFinding_IsStoredByTheFlow()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new EvaluateIssuesInput(TestClaims.Start.AddDays(-10), TestClaims.Actor, TestClaims.Reference());

        new EvaluateIssuesFlow().Execute(claim, input);

        claim.BlockingIssues.Select(issue => issue.Finding.Code).Should().Equal(IsLossDateNotInFuture.Code);
    }

    [Fact]
    [Trait("Rule", IsLossDateNotInFuture.Code)]
    public void ALossDateInTheFuture_RaisesAnIssue()
    {
        var claim = TestClaims.WithLoss(TestClaims.Start.AddDays(2));

        claim.BlockingIssues.Select(issue => issue.Finding.Code).Should().Equal(IsLossDateNotInFuture.Code);
    }

    [Fact]
    [Trait("Rule", IsLossDateNotInFuture.Code)]
    public void AnIssueThatComesBack_IsRaisedAgainAsANewIssue()
    {
        var claim = TestClaims.WithLoss(TestClaims.Start.AddDays(2));
        TestClaims.SetLoss(claim, TestClaims.Loss(TestClaims.Start.AddDays(-1), TestClaims.Description));
        TestClaims.SetLoss(claim, TestClaims.Loss(TestClaims.Start.AddDays(2), TestClaims.Description));

        var futureIssues = claim.Issues
            .Where(issue => issue.Finding.Code == IsLossDateNotInFuture.Code)
            .ToList();

        futureIssues.Should().HaveCount(2);
        futureIssues.Count(issue => issue.IsCurrent).Should().Be(1);
    }

    [Fact]
    public void ACompleteClaim_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new EvaluateIssuesInput(TestClaims.Later, TestClaims.Actor, TestClaims.Reference());

        var outcome = new EvaluateIssuesFlow().Execute(claim, input);

        outcome.Result.HasNoFindings.Should().BeTrue();
    }

    [Fact]
    public void Execute_WhenTheInputTimeIsBeforeTheLossDate_ReportsTheFutureLossDate()
    {
        var claim = TestClaims.WithLoss(TestClaims.Later);
        var input = new EvaluateIssuesInput(TestClaims.Start, TestClaims.Actor, TestClaims.Reference());

        var outcome = new EvaluateIssuesFlow().Execute(claim, input);

        outcome.Blocking.Select(finding => finding.Code).Should().Equal(IsLossDateNotInFuture.Code);
    }

    [Fact]
    public void Execute_WhenTheInputTimeIsTheLossDate_ReportsNothing()
    {
        var claim = TestClaims.WithLoss(TestClaims.Later);
        var input = new EvaluateIssuesInput(TestClaims.Later, TestClaims.Actor, TestClaims.Reference());

        var outcome = new EvaluateIssuesFlow().Execute(claim, input);

        outcome.Result.HasNoFindings.Should().BeTrue();
    }
}
