using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Claims.Issues.Flows;
using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Issues.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Flows;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Flows;

public sealed class WaiveIssueOperationTests
{
    [Fact]
    [Trait("Rule", IsIssueWaivable.Code)]
    public void Waive_ResolvesTheCriticalIssueWithoutClearingIt()
    {
        var claim = TestClaims.Empty();
        var issue = claim.Issues.First(candidate => candidate.Finding.Code == HasClaimant.Code);
        var input = new WaiveIssueInput(issue.Id, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = WaiveIssueOperation.Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        issue.State.Should().BeOfType<IssueWaivedState>();
        issue.ResolvedBy.Should().Be(TestClaims.Actor);
        issue.ResolvedAt.Should().Be(TestClaims.Later);
        claim.BlockingIssues.Should().HaveCount(3);
    }

    [Fact]
    [Trait("Rule", IsIssueWaivable.Code)]
    public void Waive_AnAlreadyWaivedIssue_Fails()
    {
        var claim = TestClaims.Empty();
        var issueId = claim.Issues[0].Id;
        var input = new WaiveIssueInput(issueId, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());
        WaiveIssueOperation.Execute(claim, input);

        var outcome = WaiveIssueOperation.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsIssueWaivable.Code);
    }

    [Fact]
    [Trait("Rule", IsIssueWaivable.Code)]
    public void Waive_AnUnknownIssue_Fails()
    {
        var claim = TestClaims.Empty();
        var input = new WaiveIssueInput(Guid.NewGuid(), TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = WaiveIssueOperation.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsIssueWaivable.Code);
    }

    [Fact]
    public void AWaivedIssue_StaysWaivedWhileTheProblemRemains()
    {
        var claim = TestClaims.Empty();
        var issue = claim.Issues.First(candidate => candidate.Finding.Code == HasClaimant.Code);
        WaiveIssueOperation.Execute(claim, new WaiveIssueInput(
            issue.Id,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference()));
        var issuesBefore = claim.Issues.Count;
        var input = new SetSeverityInput(
            ClaimSeverity.Critical,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        SetSeverityOperation.Execute(claim, input);

        issue.State.Should().BeOfType<IssueWaivedState>();
        claim.Issues.Should().HaveCount(issuesBefore);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Waive_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new WaiveIssueInput(claim.Issues[0].Id, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = WaiveIssueOperation.Execute(claim, input);

        outcome.Blocking.Select(finding => finding.Code).Should().Contain(IsClaimEditable.Code);
    }
}
