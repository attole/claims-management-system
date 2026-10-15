using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Flows;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Flows;

public sealed class UpdateLossDetailsOperationTests
{
    [Fact]
    public void UpdateLossDetails_ReplacesTheLoss()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var details = new LossDetails(new LossDate(TestClaims.Start.AddDays(-1)), "Fire", "FIRE");
        var input = new UpdateLossDetailsInput(details, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        UpdateLossDetailsOperation.Execute(claim, input);

        claim.LossEvent.Details.Should().Be(details);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void UpdateLossDetails_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-1), TestClaims.Description);
        var input = new UpdateLossDetailsInput(loss, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = UpdateLossDetailsOperation.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }
}
