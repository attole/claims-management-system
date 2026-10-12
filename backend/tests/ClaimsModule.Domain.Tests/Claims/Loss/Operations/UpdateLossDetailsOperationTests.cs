using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Operations;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Operations;

public sealed class UpdateLossDetailsOperationTests
{
    [Fact]
    public void UpdateLossDetails_ReplacesTheLoss()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant());
        LossDetails details = new(new LossDate(TestClaims.Start.AddDays(-1)), "Fire", "FIRE");

        new UpdateLossDetailsOperation().Execute(claim, new UpdateLossDetailsInput(details, TestClaims.Later));

        claim.LossEvent.Details.Should().Be(details);
        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }
}
