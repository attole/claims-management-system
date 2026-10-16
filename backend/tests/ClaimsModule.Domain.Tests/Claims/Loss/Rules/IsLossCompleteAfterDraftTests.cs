using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Rules;

public sealed class IsLossCompleteAfterDraftTests
{
    [Fact]
    [Trait("Rule", IsLossCompleteAfterDraft.Code)]
    public void Check_ForADraft_PassesWhateverTheLoss()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = CreateRule(LossDetails.Empty).Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsLossCompleteAfterDraft.Code)]
    public void Check_AfterDraftWithACompleteLoss_Passes()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-1), TestClaims.Description);

        var result = CreateRule(loss).Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsLossCompleteAfterDraft.Code)]
    public void Check_AfterDraftWithALossOutsideThePolicy_PassesWithoutTheWarning()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var loss = TestClaims.Loss(TestClaims.OutsidePolicyPeriod, TestClaims.Description);

        var result = CreateRule(loss).Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsLossCompleteAfterDraft.Code)]
    public void Check_AfterDraftWithAWaivedProblem_Passes()
    {
        var claim = WaivedShortDescriptionAfterDraft();
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-3), "Leak");

        var result = CreateRule(loss).Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsLossCompleteAfterDraft.Code)]
    public void Check_AfterDraftWithANewProblemOfAWaivedCode_ReturnsIt()
    {
        var claim = WaivedShortDescriptionAfterDraft();
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-3), "Drip");

        var result = CreateRule(loss).Check(claim);

        result.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsDescriptionLongEnough.Code);
    }

    private static IsLossCompleteAfterDraft CreateRule(LossDetails details)
    {
        return new IsLossCompleteAfterDraft(details, TestClaims.LossReference());
    }

    private static Claim WaivedShortDescriptionAfterDraft()
    {
        var claim = TestClaims.WithDescription("Leak");
        TestClaims.WaiveIssue(claim, IsDescriptionLongEnough.Code);
        TestClaims.MoveTo<ClaimOpenState>(claim);

        return claim;
    }
}
