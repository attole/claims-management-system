using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Flows;
using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Flows;

public sealed class EvaluateIssuesOperationTests
{
    private static readonly string[] _emptyClaimCodes =

    [
        IsLossDatePresent.Code,
        IsDescriptionLongEnough.Code,
        IsCauseCodeActive.Code,
        HasClaimant.Code,
    ];
    [Fact]
    [Trait("Rule", "S:VAL-Severity")]
    public void Run_ForAnEmptyClaim_SucceedsAndKeepsACriticalIssueForWhatIsMissing()
    {
        var claim = TestClaims.Empty();
        var input = new EvaluateIssuesInput(TestClaims.LossReference());

        var outcome = EvaluateIssuesOperation.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        UnresolvedCriticalCodes(claim).Should().BeEquivalentTo(_emptyClaimCodes);
    }

    [Fact]
    public void Run_WhenRunTwice_StoresNothingNew()
    {
        var claim = TestClaims.Empty();
        var input = new EvaluateIssuesInput(TestClaims.LossReference());
        EvaluateIssuesOperation.Run(claim, input);
        var issuesBefore = claim.Issues.Count;

        var outcome = EvaluateIssuesOperation.Run(claim, input);

        claim.Issues.Should().HaveCount(issuesBefore);
        outcome.Value.Raised.Should().BeEmpty();
        outcome.Value.Cleared.Should().BeEmpty();
    }

    [Fact]
    public void AFixedProblem_ClearsItsIssue()
    {
        var claim = TestClaims.Empty();
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-1), TestClaims.Description);
        var update = new UpdateLossDetailsInput(loss, TestClaims.LossReference());
        TestClaims.ChangeLoss(claim, update);
        var input = new EvaluateIssuesInput(TestClaims.LossReference());

        var outcome = EvaluateIssuesOperation.Run(claim, input);

        UnresolvedCriticalCodes(claim).Should().Equal(HasClaimant.Code);
        string[] clearedCodes = [IsLossDatePresent.Code, IsDescriptionLongEnough.Code, IsCauseCodeActive.Code];
        outcome.Value.Cleared.Select(issue => issue.Finding.Code).Should().BeEquivalentTo(clearedCodes);
        outcome.Value.Raised.Should().BeEmpty();
    }

    [Fact]
    public void ANewFinding_IsStoredAsAnIssue()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var reference = TestClaims.ReferenceWithCauseCodes("OTHER");
        var input = new EvaluateIssuesInput(reference);

        EvaluateIssuesOperation.Run(claim, input);

        UnresolvedCriticalCodes(claim).Should().Equal(IsCauseCodeActive.Code);
    }

    [Fact]
    [Trait("Rule", IsLossDateNotAfterReport.Code)]
    public void Run_WhenAnIssueComesBack_RaisesItAgainAsANewIssue()
    {
        var claim = TestClaims.WithLoss(TestClaims.Start.AddDays(2));
        var input = new EvaluateIssuesInput(TestClaims.LossReference());
        var past = TestClaims.Loss(TestClaims.Start.AddDays(-1), TestClaims.Description);
        var future = TestClaims.Loss(TestClaims.Start.AddDays(2), TestClaims.Description);
        TestClaims.ChangeLoss(claim, new UpdateLossDetailsInput(past, TestClaims.LossReference()));
        EvaluateIssuesOperation.Run(claim, input);
        TestClaims.ChangeLoss(claim, new UpdateLossDetailsInput(future, TestClaims.LossReference()));

        EvaluateIssuesOperation.Run(claim, input);

        var futureIssues = claim.Issues
            .Where(issue => issue.Finding.HasCode(IsLossDateNotAfterReport.Code))
            .ToList();
        futureIssues.Should().HaveCount(2);
        futureIssues.Count(issue => issue.State.IsCurrent).Should().Be(1);
    }

    [Fact]
    public void Run_ForACompleteClaim_LeavesNoCurrentIssue()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new EvaluateIssuesInput(TestClaims.LossReference());

        EvaluateIssuesOperation.Run(claim, input);

        claim.Issues.Should().OnlyContain(issue => !issue.State.IsCurrent);
    }

    [Fact]
    public void Run_ForAWaivedIssueWhoseProblemRemains_KeepsItWaived()
    {
        var claim = TestClaims.Empty();
        var issue = claim.Issues.First(issue => issue.Finding.HasCode(IsDescriptionLongEnough.Code));
        TestClaims.WaiveIssue(claim, IsDescriptionLongEnough.Code);
        var issuesBefore = claim.Issues.Count;
        var input = new EvaluateIssuesInput(TestClaims.LossReference());

        EvaluateIssuesOperation.Run(claim, input);

        issue.State.Should().BeOfType<IssueWaivedState>();
        claim.Issues.Should().HaveCount(issuesBefore);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Run_OnAClosedClaim_ReturnsTheFindingAndChangesNoIssue()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var issuesBefore = claim.Issues.Count;
        var reference = TestClaims.ReferenceWithCauseCodes("OTHER");
        var input = new EvaluateIssuesInput(reference);

        var outcome = EvaluateIssuesOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
        claim.Issues.Should().HaveCount(issuesBefore);
    }

    [Fact]
    [Trait("Rule", "S:BR-C-02")]
    public void Run_WhenTheFactsBehindAnAcknowledgedWarningChange_RaisesItAgain()
    {
        var claim = TestClaims.WithLoss(TestClaims.OutsidePolicyPeriod);
        TestClaims.AcknowledgeIssue(claim, IsLossDateWithinPolicyPeriod.Code);
        var earlier = TestClaims.Loss(TestClaims.OutsidePolicyPeriod.AddDays(-30), TestClaims.Description);
        TestClaims.ChangeLoss(claim, new UpdateLossDetailsInput(earlier, TestClaims.LossReference()));
        var input = new EvaluateIssuesInput(TestClaims.LossReference());

        EvaluateIssuesOperation.Run(claim, input);

        claim.HasUnresolvedWarning(IsLossDateWithinPolicyPeriod.Code).Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", "S:BR-C-02")]
    public void Run_WhenTheFactsBehindAnAcknowledgedWarningStay_KeepsTheAcknowledgement()
    {
        var claim = TestClaims.WithLoss(TestClaims.OutsidePolicyPeriod);
        TestClaims.AcknowledgeIssue(claim, IsLossDateWithinPolicyPeriod.Code);
        var input = new EvaluateIssuesInput(TestClaims.LossReference());

        EvaluateIssuesOperation.Run(claim, input);

        claim.HasUnresolvedWarning(IsLossDateWithinPolicyPeriod.Code).Should().BeFalse();
    }

    [Fact]
    public void Run_WithAReferenceForAnotherPolicy_Throws()
    {
        var claim = TestClaims.Empty();
        var input = new EvaluateIssuesInput(TestClaims.NoPolicyReference());

        Action act = () => EvaluateIssuesOperation.Run(claim, input);

        act.Should().Throw<InvalidOperationException>();
    }

    private static IEnumerable<string> UnresolvedCriticalCodes(Claim claim)
    {
        var codes = claim.Issues
            .Where(issue => issue.IsUnresolvedCritical)
            .Select(issue => issue.Finding.Code);

        return codes;
    }
}
