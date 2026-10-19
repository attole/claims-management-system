using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Events;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.ReserveComponents.Handlers;
using ClaimsModule.Application.ReserveComponents.History.Handlers;
using ClaimsModule.Domain.Claims.Events;
using ClaimsModule.Domain.ReserveComponents.History.Events;
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

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void AddEventHandlers_RegistersTheApprovalPostingHandler()
    {
        var provider = CreateProvider();

        var result = provider.GetServices<IAfterCommitEventHandler<ReserveHistoryApprovalStateChangedEvent>>();

        result.Should().ContainSingle(handler => handler is ReserveApprovedPostingHandler);
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void AddEventHandlers_RegistersTheRetriedPostingHandler()
    {
        var provider = CreateProvider();

        var result = provider.GetServices<IAfterCommitEventHandler<ReserveHistoryPostingStateChangedEvent>>();

        result.Should().ContainSingle(handler => handler is ReservePostingRetriedHandler);
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IReserveComponentRepository>());
        services.AddSingleton(Mock.Of<IPostingScheduler>());
        services.AddEventHandlers(typeof(ApplicationServiceExtensions).Assembly);

        return services.BuildServiceProvider();
    }
}
