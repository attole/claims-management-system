using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Operations;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Operations;

public sealed class UpdateLossDetailsOperationTests
{
    [Fact]
    public void UpdateLossDetails_ReplacesTheLoss()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var details = new LossDetails(new LossDate(TestClaims.Start.AddDays(-1)), "Fire", "FIRE");
        var input = new UpdateLossDetailsInput(details, TestClaims.Actor, TestClaims.Later);

        new UpdateLossDetailsOperation().Execute(claim, input);

        claim.LossEvent.Details.Should().Be(details);
        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void UpdateLossDetails_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState(new ClaimClosedState());
        var loss = TestClaims.Loss(TestClaims.Start.AddDays(-1), TestClaims.Description);
        var input = new UpdateLossDetailsInput(loss, TestClaims.Actor, TestClaims.Later);

        var outcome = new UpdateLossDetailsOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }
}
