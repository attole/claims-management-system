using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Claims.Issues.Events;
using ClaimsModule.Domain.Claims.Loss.Operations;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Operations;
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
        claim.PopEvents();

        flow.Execute(claim, input);

        claim.Issues.Should().HaveCount(4);
        claim.PopEvents().Should().BeEmpty();
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
    public void ANewClaim_RaisesAnEventForEveryIssue()
    {
        var claim = TestClaims.Empty();

        var raised = claim.PopEvents().OfType<ValidationIssueRaisedEvent>();

        raised.Select(issue => issue.Code).Should().BeEquivalentTo(_emptyClaimCodes);
        raised.Should().OnlyContain(issue => issue.ClaimId == claim.Id && issue.ActorId == TestClaims.Actor);
    }

    [Fact]
    public void ACompleteClaim_HasNoBlockingIssueAndEveryEarlierIssueIsCleared()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        claim.BlockingIssues.Should().BeEmpty();
        claim.Issues.Should().OnlyContain(issue => !issue.IsCurrent);
    }

    [Fact]
    public void AFixedProblem_ClearsItsIssueAndRaisesAClearedEvent()
    {
        var claim = TestClaims.Empty();
        claim.PopEvents();
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-1), TestClaims.Description);
        var update = new UpdateLossDetailsInput(loss, TestClaims.Actor, TestClaims.Later);
        new UpdateLossDetailsOperation().Execute(claim, update);
        var input = new EvaluateIssuesInput(TestClaims.Later, TestClaims.Actor, TestClaims.Reference());

        new EvaluateIssuesFlow().Execute(claim, input);

        claim.BlockingIssues.Select(issue => issue.Finding.Code).Should().Equal(HasClaimant.Code);
        var cleared = claim.PopEvents().OfType<ValidationIssueClearedEvent>().ToList();
        string[] clearedCodes = [IsLossDatePresent.Code, IsDescriptionLongEnough.Code, IsCauseCodeActive.Code];
        cleared.Select(issue => issue.Code).Should().BeEquivalentTo(clearedCodes);
        cleared.Should().OnlyContain(issue => issue.ActorId == TestClaims.Actor);
    }

    [Fact]
    public void AnUnchangedClaim_RaisesNothingNewWhenAnotherFieldChanges()
    {
        var claim = TestClaims.Empty();
        claim.PopEvents();
        var input = new SetSeverityInput(ClaimSeverity.Critical, TestClaims.Actor, TestClaims.Later);

        new SetSeverityOperation().Execute(claim, input);

        claim.PopEvents().OfType<ValidationIssueRaisedEvent>().Should().BeEmpty();
        claim.Issues.Should().HaveCount(4);
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
    public void RaisingAnIssue_TouchesTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var loss = TestClaims.Loss(TestClaims.Later.AddDays(5), TestClaims.Description);
        var update = new UpdateLossDetailsInput(loss, TestClaims.Actor, TestClaims.Start);
        new UpdateLossDetailsOperation().Execute(claim, update);
        var input = new EvaluateIssuesInput(TestClaims.Later, TestClaims.Actor, TestClaims.Reference());

        new EvaluateIssuesFlow().Execute(claim, input);

        claim.UpdatedAt.Should().Be(TestClaims.Later);
        claim.Issues.Count(issue => issue.IsCurrent).Should().Be(1);
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
