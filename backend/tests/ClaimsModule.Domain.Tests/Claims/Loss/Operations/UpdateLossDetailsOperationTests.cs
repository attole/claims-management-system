using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Operations;
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
}
