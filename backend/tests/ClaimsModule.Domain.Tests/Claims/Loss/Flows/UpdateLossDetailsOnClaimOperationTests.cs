using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Flows;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Flows;

public sealed class UpdateLossDetailsOnClaimOperationTests
{
    [Fact]
    public void Run_ReplacesTheLoss()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var details = new LossDetails(new LossDate(TestClaims.Start.AddDays(-1)), "Fire", "FIRE");
        var input = new UpdateLossDetailsInput(details, TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);

        var outcome = UpdateLossDetailsOnClaimOperation.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.LossEvent.Details.Should().Be(details);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Run_WhenTheClaimIsClosed_ReturnsTheFinding()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-1), TestClaims.Description);
        var input = new UpdateLossDetailsInput(loss, TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);

        var outcome = UpdateLossDetailsOnClaimOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }

    [Fact]
    [Trait("Rule", IsLossCompleteAfterDraft.Code)]
    public void Run_AfterDraftWithAnEmptyLoss_FailsAndKeepsTheLoss()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var before = claim.LossEvent.Details;
        var input = new UpdateLossDetailsInput(
            LossDetails.Empty,
            TestClaims.Period(),
            TestClaims.Codes(),
            TestClaims.Start);

        var outcome = UpdateLossDetailsOnClaimOperation.Run(claim, input);

        outcome.CriticalFindings.Select(finding => finding.Code).Should().Contain(IsLossDatePresent.Code);
        claim.LossEvent.Details.Should().Be(before);
    }
}
