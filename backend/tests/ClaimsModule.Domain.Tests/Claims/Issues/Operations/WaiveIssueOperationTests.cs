using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Claims.Issues.Events;
using ClaimsModule.Domain.Claims.Issues.Operations;
using ClaimsModule.Domain.Claims.Issues.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Operations;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Operations;

public sealed class WaiveIssueOperationTests
{
    [Fact]
    [Trait("Rule", IsIssueWaivable.Code)]
    public void Waive_ResolvesTheCriticalIssueWithoutClearingIt()
    {
        var claim = TestClaims.Empty();
        var issue = claim.Issues.First(candidate => candidate.Finding.Code == HasClaimant.Code);
        var input = new WaiveIssueInput(issue.Id, TestClaims.Actor, TestClaims.Later);

        var outcome = new WaiveIssueOperation().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        issue.State.Should().BeOfType<IssueWaivedState>();
        issue.ResolvedBy.Should().Be(TestClaims.Actor);
        issue.ResolvedAt.Should().Be(TestClaims.Later);
        claim.BlockingIssues.Should().HaveCount(3);
    }

    [Fact]
    public void Waive_RaisesAWaivedEventForTheIssue()
    {
        var claim = TestClaims.Empty();
        var issue = claim.Issues[0];
        claim.PopEvents();
        var input = new WaiveIssueInput(issue.Id, TestClaims.Actor, TestClaims.Later);

        new WaiveIssueOperation().Execute(claim, input);

        var waived = claim.PopEvents().OfType<ValidationIssueWaivedEvent>().Should().ContainSingle().Subject;
        waived.IssueId.Should().Be(issue.Id);
        waived.Code.Should().Be(issue.Finding.Code);
        waived.ActorId.Should().Be(TestClaims.Actor);
    }

    [Fact]
    [Trait("Rule", IsIssueWaivable.Code)]
    public void Waive_AnAlreadyWaivedIssue_Fails()
    {
        var claim = TestClaims.Empty();
        var issueId = claim.Issues[0].Id;
        var input = new WaiveIssueInput(issueId, TestClaims.Actor, TestClaims.Later);
        new WaiveIssueOperation().Execute(claim, input);

        var outcome = new WaiveIssueOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsIssueWaivable.Code);
    }

    [Fact]
    [Trait("Rule", IsIssueWaivable.Code)]
    public void Waive_AnUnknownIssue_Fails()
    {
        var claim = TestClaims.Empty();
        var input = new WaiveIssueInput(Guid.NewGuid(), TestClaims.Actor, TestClaims.Later);

        var outcome = new WaiveIssueOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsIssueWaivable.Code);
    }

    [Fact]
    public void AWaivedIssue_StaysWaivedWhileTheProblemRemains()
    {
        var claim = TestClaims.Empty();
        var issue = claim.Issues.First(candidate => candidate.Finding.Code == HasClaimant.Code);
        new WaiveIssueOperation().Execute(claim, new WaiveIssueInput(issue.Id, TestClaims.Actor, TestClaims.Later));
        var input = new SetSeverityInput(ClaimSeverity.Critical, TestClaims.Actor, TestClaims.Later);

        new SetSeverityOperation().Execute(claim, input);

        issue.State.Should().BeOfType<IssueWaivedState>();
        claim.Issues.Should().HaveCount(4);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Waive_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState(new ClaimClosedState());
        var input = new WaiveIssueInput(claim.Issues[0].Id, TestClaims.Actor, TestClaims.Later);

        var outcome = new WaiveIssueOperation().Execute(claim, input);

        outcome.Blocking.Select(finding => finding.Code).Should().Contain(IsClaimEditable.Code);
    }
}
