using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Claims.Issues.Operations;
using ClaimsModule.Domain.Claims.Issues.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Operations;

public sealed class AcknowledgeIssueOperationTests
{
    [Fact]
    [Trait("Rule", "S:BR-C-02")]
    public void Execute_ForThePolicyPeriodWarning_AcknowledgesIt()
    {
        var claim = TestClaims.WithLoss(TestClaims.OutsidePolicyPeriod);
        var issue = claim.Issues.Single(candidate => candidate.Finding.Code == IsLossDateWithinPolicyPeriod.Code);
        var input = new AcknowledgeIssueInput(issue.Id, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = new AcknowledgeIssueOperation().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        issue.State.Should().BeOfType<IssueAcknowledgedState>();
    }

    [Fact]
    [Trait("Rule", HasLinkedPolicy.Code)]
    public void Execute_ForAnUnresolvedWarning_AcknowledgesItAndRecordsTheActor()
    {
        var claim = TestClaims.WithPolicyWarning();
        var issue = claim.Issues.Single(candidate => candidate.Finding.Code == HasLinkedPolicy.Code);
        var input = new AcknowledgeIssueInput(issue.Id, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = new AcknowledgeIssueOperation().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        issue.State.Should().BeOfType<IssueAcknowledgedState>();
        issue.ResolvedBy.Should().Be(TestClaims.Actor);
        issue.ResolvedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    [Trait("Rule", IsIssueAcknowledgeable.Code)]
    public void Execute_ForACriticalIssue_ReturnsTheFindingAndKeepsTheIssue()
    {
        var claim = TestClaims.WithPolicyWarning();
        var issue = claim.Issues.Single(candidate => candidate.Finding.Code == IsLossDatePresent.Code);
        var input = new AcknowledgeIssueInput(issue.Id, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = new AcknowledgeIssueOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsIssueAcknowledgeable.Code);
        issue.State.Should().BeOfType<IssueRaisedState>();
    }

    [Fact]
    [Trait("Rule", IsIssueAcknowledgeable.Code)]
    public void Execute_Twice_ReturnsTheFindingTheSecondTime()
    {
        var claim = TestClaims.WithPolicyWarning();
        var issue = claim.Issues.Single(candidate => candidate.Finding.Code == HasLinkedPolicy.Code);
        var input = new AcknowledgeIssueInput(issue.Id, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());
        new AcknowledgeIssueOperation().Execute(claim, input);

        var outcome = new AcknowledgeIssueOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsIssueAcknowledgeable.Code);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Execute_OnAClosedClaim_ReturnsTheFinding()
    {
        var claim = TestClaims.InState(new ClaimClosedState());
        var input = new AcknowledgeIssueInput(
            Guid.NewGuid(),
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = new AcknowledgeIssueOperation().Execute(claim, input);

        outcome.Blocking.Select(finding => finding.Code).Should().Contain(IsClaimEditable.Code);
    }
}
