using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.Flows;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.Flows;

public sealed class CloseReserveComponentTransitionTests
{
    [Fact]
    public void Run_FromActive_ClosesTheComponent()
    {
        var component = TestReserves.Component();

        var outcome = CloseReserveComponentTransition.Run(component, NoInput.Value);

        outcome.Value.To.Should().BeOfType<ReserveComponentClosedState>();
        component.State.Should().BeOfType<ReserveComponentClosedState>();
    }

    [Fact]
    public void Run_FromClosed_ReturnsNotPermittedAndChangesNothing()
    {
        var component = TestReserves.Component();
        CloseReserveComponentTransition.Run(component, NoInput.Value);

        var outcome = CloseReserveComponentTransition.Run(component, NoInput.Value);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(CloseReserveComponentTransition.NotPermittedCode);
        component.State.Should().BeOfType<ReserveComponentClosedState>();
    }
}
