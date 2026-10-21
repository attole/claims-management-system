using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Flows;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Flows;

public sealed class OpenClaimTransitionTests
{
    [Fact]
    [Trait("Rule", "S:ST-Draft-Open")]
    public void Run_FromDraftWhenTheClaimIsComplete_OpensIt()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new ClaimTransitionInput(TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);

        var outcome = OpenClaimTransition.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    [Trait("Rule", HasNoCriticalIssuesForOpen.Code)]
    [Trait("Rule", HasClaimant.Code)]
    public void Run_FromDraftWithUnresolvedCriticalIssues_ReturnsEveryFinding()
    {
        var claim = TestClaims.Empty();
        var input = new ClaimTransitionInput(TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);

        var outcome = OpenClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Select(finding => finding.Code)
            .Should().BeEquivalentTo(HasNoCriticalIssuesForOpen.Code, HasClaimant.Code, HasAssignedHandler.Code);
        claim.State.Should().BeOfType<ClaimDraftState>();
    }

    [Fact]
    [Trait("Rule", HasNoUnacknowledgedPolicyWarning.Code)]
    public void Run_FromDraftWithAnUnacknowledgedPolicyPeriodWarning_ReturnsTheFinding()
    {
        var claim = TestClaims.WithLoss(TestClaims.OutsidePolicyPeriod);
        var input = new ClaimTransitionInput(TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);

        var outcome = OpenClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasNoUnacknowledgedPolicyWarning.Code);
        claim.State.Should().BeOfType<ClaimDraftState>();
    }

    [Fact]
    [Trait("Rule", "S:ST-UnderInvestigation-Open")]
    public void Run_FromUnderInvestigation_OpensIt()
    {
        var claim = TestClaims.InState<ClaimUnderInvestigationState>();
        var input = new ClaimTransitionInput(TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);

        var outcome = OpenClaimTransition.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    [Trait("Rule", "S:BR-ST-02")]
    public void Run_FromUnderInvestigationWithoutAClaimant_ReturnsTheFindingAndKeepsTheState()
    {
        var claim = TestClaims.InStateWithoutClaimant<ClaimUnderInvestigationState>();
        var input = new ClaimTransitionInput(TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);

        var outcome = OpenClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Select(finding => finding.Code).Should().Contain(HasClaimant.Code);
        claim.State.Should().BeOfType<ClaimUnderInvestigationState>();
    }

    [Fact]
    [Trait("Rule", "S:BR-ST-02")]
    [Trait("Rule", HasNoUnacknowledgedPolicyWarning.Code)]
    public void Run_FromUnderInvestigationWithAnUnacknowledgedPolicyPeriodWarning_ReturnsTheFinding()
    {
        var claim = TestClaims.InState<ClaimUnderInvestigationState>();
        TestClaims.SetLoss(claim, TestClaims.Loss(TestClaims.OutsidePolicyPeriod, TestClaims.Description));
        var input = new ClaimTransitionInput(TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);

        var outcome = OpenClaimTransition.Run(claim, input);

        var codes = outcome.CriticalFindings.Select(finding => finding.Code);
        codes.Should().Contain(HasNoUnacknowledgedPolicyWarning.Code);
        claim.State.Should().BeOfType<ClaimUnderInvestigationState>();
    }

    [Fact]
    [Trait("Rule", "S:ST-Reopened-Open")]
    public void Run_FromReopened_OpensItWithoutFurtherRules()
    {
        var claim = TestClaims.InState<ClaimReopenedState>();
        var input = new ClaimTransitionInput(TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);

        var outcome = OpenClaimTransition.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.State.Should().BeOfType<ClaimOpenState>();
    }

    [Fact]
    [Trait("Rule", "S:ST-Other-Invalid")]
    public void Run_FromAnUndeclaredSource_ReturnsNotPermitted()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = new ClaimTransitionInput(TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);

        var outcome = OpenClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(OpenClaimTransition.NotPermittedCode);
    }

    [Fact]
    [Trait("Rule", HasAssignedHandler.Code)]
    public void Run_FromDraftWithoutAHandler_ReturnsTheFindingAndKeepsTheDraft()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var unassign = new AssignHandlerInput(Guid.Empty, TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);
        claim.AssignHandler(unassign);
        var input = new ClaimTransitionInput(TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);

        var outcome = OpenClaimTransition.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasAssignedHandler.Code);
        claim.State.Should().BeOfType<ClaimDraftState>();
    }
}
