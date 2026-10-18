using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Flows;

public sealed class ApproveHistoryOnReserveComponentOperationTests
{
    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Run_ApprovesTheRowWithTheBalancesOfTheComponent()
    {
        var component = TestReserves.ComponentWithApprovedBalance(5_000m);
        var history = component.SubmitHistory(TestReserves.SubmitInput(50_000m)).Value;
        var input = new ApproveHistoryInput(
            history.Id,
            0m,
            false,
            TestClaims.Supervisor,
            TestReserves.Approver,
            TestClaims.Later);

        var outcome = ApproveHistoryOnReserveComponentOperation.Run(component, input);

        outcome.IsSuccess.Should().BeTrue();
        history.ApprovalState.Should().BeOfType<ReserveApprovedState>();
        history.Balances.PreviousBalance.Should().Be(5_000m);
        history.Balances.NewBalance.Should().Be(55_000m);
    }

    [Fact]
    public void Run_ForAnUnknownRow_Throws()
    {
        var component = TestReserves.Component();
        var input = new ApproveHistoryInput(
            Guid.NewGuid(),
            0m,
            false,
            TestClaims.Supervisor,
            TestReserves.Approver,
            TestClaims.Later);

        Action act = () => ApproveHistoryOnReserveComponentOperation.Run(component, input);

        act.Should().Throw<InvalidOperationException>();
    }
}
