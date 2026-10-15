using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Checks;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Rules;

public sealed class IsLossCompleteAfterDraftTests
{
    [Fact]
    [Trait("Rule", IsLossCompleteAfterDraft.Code)]
    public void Check_ForADraft_PassesWhateverTheLoss()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = CreateRule(LossDetails.Empty, ClaimStateKind.Draft);

        var result = rule.Check(claim.LossEvent);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsLossCompleteAfterDraft.Code)]
    public void Check_AfterDraftWithACompleteLoss_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-1), TestClaims.Description);
        var rule = CreateRule(loss, ClaimStateKind.Open);

        var result = rule.Check(claim.LossEvent);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsLossCompleteAfterDraft.Code)]
    public void Check_AfterDraftWithAnEmptyLoss_ReportsEveryMissingValue()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = CreateRule(LossDetails.Empty, ClaimStateKind.Open);

        var result = rule.Check(claim.LossEvent);

        result.Blocking.Select(finding => finding.Code).Should().BeEquivalentTo(
            IsLossDatePresent.Code,
            IsDescriptionLongEnough.Code,
            IsCauseCodeActive.Code);
    }

    [Fact]
    [Trait("Rule", IsLossCompleteAfterDraft.Code)]
    public void Check_AfterDraftWithALossAfterTheReport_ReportsIt()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var loss = TestClaims.Loss(TestClaims.Start.AddMinutes(30), TestClaims.Description);
        var rule = CreateRule(loss, ClaimStateKind.Open);

        var result = rule.Check(claim.LossEvent);

        result.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsLossDateNotAfterReport.Code);
    }

    private static IsLossCompleteAfterDraft CreateRule(LossDetails details, ClaimStateKind claimState)
    {
        var change = new ChangeLossInput(
            details,
            claimState,
            TestClaims.Reference(),
            TestClaims.Later,
            TestClaims.Actor);

        return new IsLossCompleteAfterDraft(change);
    }
}
