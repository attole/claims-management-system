using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Flows;

public sealed class RetractHistoryOnReserveComponentOperationTests
{
    [Fact]
    [Trait("Rule", "S:RES-PendingImmutable")]
    public void Run_CancelsThePendingRow()
    {
        var component = TestReserves.Component();
        var history = component.SubmitHistory(TestReserves.SubmitInput(50_000m)).Value;
        var input = new RetractHistoryInput(history.Id, TestReserves.Submitter, TestClaims.Later);

        var outcome = RetractHistoryOnReserveComponentOperation.Run(component, input);

        outcome.IsSuccess.Should().BeTrue();
        history.ApprovalState.Should().BeOfType<ReserveCancelledState>();
    }

    [Fact]
    public void Run_ForAnUnknownRow_Throws()
    {
        var component = TestReserves.Component();
        var input = new RetractHistoryInput(Guid.NewGuid(), TestReserves.Submitter, TestClaims.Later);

        Action act = () => RetractHistoryOnReserveComponentOperation.Run(component, input);

        act.Should().Throw<InvalidOperationException>();
    }
}
