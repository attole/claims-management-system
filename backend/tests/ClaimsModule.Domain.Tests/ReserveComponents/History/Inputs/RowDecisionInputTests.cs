using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Inputs;

public sealed class RowDecisionInputTests
{
    [Fact]
    public void ReviewerId_IsTheActor()
    {
        var input = new RowDecisionInput(TestReserves.Approver, TestClaims.Later);

        input.ReviewerId.Should().Be(TestReserves.Approver);
    }
}
