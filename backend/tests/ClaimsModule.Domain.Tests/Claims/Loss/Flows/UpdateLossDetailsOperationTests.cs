using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Flows;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Loss.Flows;

public sealed class UpdateLossDetailsOperationTests
{
    [Fact]
    public void Run_ReplacesTheDetailsOfTheLoss()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var details = new LossDetails(new LossDate(TestClaims.Start.AddDays(-1)), "Fire", "FIRE");
        var input = new LossDetailsInput(details);

        var outcome = UpdateLossDetailsOperation.Run(claim.LossEvent, input);

        outcome.Value.Details.Should().Be(details);
    }
}
