using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Flows;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Flows;

public sealed class UpdateLossDetailsOperationTests
{
    [Fact]
    public void UpdateLossDetails_ReplacesTheLoss()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var details = new LossDetails(new LossDate(TestClaims.Start.AddDays(-1)), "Fire", "FIRE");
        var change = CreateChange(details, ClaimStateKind.Draft);

        UpdateLossDetailsOperation.Execute(claim.LossEvent, change);

        claim.LossEvent.Details.Should().Be(details);
    }

    [Fact]
    [Trait("Rule", IsLossCompleteAfterDraft.Code)]
    public void UpdateLossDetails_OfADraftWithAnEmptyLoss_Succeeds()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var change = CreateChange(LossDetails.Empty, ClaimStateKind.Draft);

        var outcome = UpdateLossDetailsOperation.Execute(claim.LossEvent, change);

        outcome.IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", IsLossCompleteAfterDraft.Code)]
    public void UpdateLossDetails_AfterDraftWithAnEmptyLoss_FailsAndKeepsTheLoss()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var before = claim.LossEvent.Details;
        var change = CreateChange(LossDetails.Empty, ClaimStateKind.Open);

        var outcome = UpdateLossDetailsOperation.Execute(claim.LossEvent, change);

        outcome.Blocking.Should().NotBeEmpty();
        claim.LossEvent.Details.Should().Be(before);
    }

    private static ChangeLossInput CreateChange(LossDetails details, ClaimStateKind claimState)
    {
        return new ChangeLossInput(
            details,
            claimState,
            TestClaims.Reference(),
            TestClaims.Later,
            TestClaims.Actor);
    }
}
