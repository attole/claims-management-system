using ClaimsModule.Application.Events;
using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Domain.Claims.Events;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Application.Tests.Events;

public sealed class EventHandlersServiceExtensionsTests
{
    [Fact]
    [Trait("Rule", "S:RES-Components")]
    public void AddEventHandlers_RegistersTheBeforeCommitHandlersOfTheAssembly()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IReserveComponentRepository>());
        services.AddEventHandlers(typeof(ApplicationServiceExtensions).Assembly);
        var provider = services.BuildServiceProvider();

        var result = provider.GetServices<IBeforeCommitEventHandler<ClaimStateChangedEvent>>();

        result.Should().ContainSingle(handler => handler is ClaimStateChangedReserveComponentHandler);
    }
}
