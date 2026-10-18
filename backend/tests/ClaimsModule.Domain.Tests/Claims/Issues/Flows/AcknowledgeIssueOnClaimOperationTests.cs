using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Flows;
using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Issues.Flows;

public sealed class AcknowledgeIssueOnClaimOperationTests
{
    [Fact]
    [Trait("Rule", "S:BR-C-02")]
    public void Run_ForThePolicyPeriodWarning_AcknowledgesIt()
    {
        var claim = TestClaims.WithLoss(TestClaims.OutsidePolicyPeriod);
        var issue = claim.Issues.Single(issue => issue.Finding.HasCode(IsLossDateWithinPolicyPeriod.Code));
        var input = new AcknowledgeIssueInput(
            issue.Id,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Period(),
            TestClaims.Codes());

        var outcome = AcknowledgeIssueOnClaimOperation.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        issue.State.Should().BeOfType<IssueAcknowledgedState>();
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Run_OnAClosedClaim_ReturnsTheFinding()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new AcknowledgeIssueInput(
            Guid.NewGuid(),
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Period(),
            TestClaims.Codes());

        var outcome = AcknowledgeIssueOnClaimOperation.Run(claim, input);

        outcome.CriticalFindings.Select(finding => finding.Code).Should().Contain(IsClaimEditable.Code);
    }

    [Fact]
    public void Run_StampedBeforeTheClaimWasReported_Throws()
    {
        var claim = TestClaims.WithLoss(TestClaims.OutsidePolicyPeriod);
        var issue = claim.Issues.Single(issue => issue.Finding.HasCode(IsLossDateWithinPolicyPeriod.Code));
        var earlier = TestClaims.Start.AddDays(-1);
        var input = new AcknowledgeIssueInput(
            issue.Id,
            TestClaims.Actor,
            earlier,
            TestClaims.Period(),
            TestClaims.Codes());

        Action act = () => AcknowledgeIssueOnClaimOperation.Run(claim, input);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
