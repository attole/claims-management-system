using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.Flows;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.Flows;

public sealed class ReopenReserveComponentTransitionTests
{
    [Fact]
    public void Run_FromClosed_ReturnsTheComponentToActive()
    {
        var component = TestReserves.Component();
        CloseReserveComponentTransition.Run(component, NoInput.Value);

        var outcome = ReopenReserveComponentTransition.Run(component, NoInput.Value);

        outcome.Value.To.Should().BeOfType<ReserveComponentActiveState>();
        component.State.Should().BeOfType<ReserveComponentActiveState>();
    }

    [Fact]
    public void Run_FromActive_ReturnsNotPermittedAndChangesNothing()
    {
        var component = TestReserves.Component();

        var outcome = ReopenReserveComponentTransition.Run(component, NoInput.Value);

        outcome.CriticalFindings.Should().ContainSingle()
            .Which.Code.Should().Be(ReopenReserveComponentTransition.NotPermittedCode);
        component.State.Should().BeOfType<ReserveComponentActiveState>();
    }
}
