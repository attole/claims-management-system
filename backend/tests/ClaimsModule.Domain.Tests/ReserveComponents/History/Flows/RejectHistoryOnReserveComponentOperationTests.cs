using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Flows;

public sealed class RejectHistoryOnReserveComponentOperationTests
{
    [Fact]
    [Trait("Rule", "S:BR-R-04")]
    public void Run_RejectsTheRowAndKeepsItInTheHistory()
    {
        var component = TestReserves.Component();
        var history = component.SubmitHistory(TestReserves.SubmitInput(50_000m)).Value;
        var input = new RejectHistoryInput(history.Id, TestClaims.Supervisor, TestReserves.Approver, TestClaims.Later)
        {
            Reason = "Not supported by the report",
        };

        var outcome = RejectHistoryOnReserveComponentOperation.Run(component, input);

        outcome.IsSuccess.Should().BeTrue();
        history.ApprovalState.Should().BeOfType<ReserveRejectedState>();
        component.History.Should().ContainSingle();
    }

    [Fact]
    public void Run_ForAnUnknownRow_Throws()
    {
        var component = TestReserves.Component();
        var input = new RejectHistoryInput(
            Guid.NewGuid(),
            TestClaims.Supervisor,
            TestReserves.Approver,
            TestClaims.Later);

        Action act = () => RejectHistoryOnReserveComponentOperation.Run(component, input);

        act.Should().Throw<InvalidOperationException>();
    }
}
