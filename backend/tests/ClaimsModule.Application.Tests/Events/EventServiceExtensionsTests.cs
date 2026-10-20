using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Handlers;
using ClaimsModule.Application.Events;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Application.Events.Services;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.ReserveComponents.Handlers;
using ClaimsModule.Application.ReserveComponents.History.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Application.Tests.Events;

public sealed class EventServiceExtensionsTests
{
    [Fact]
    [Trait("Rule", "S:RES-Components")]
    public void AddEvents_RegistersTheBeforeCommitHandlersOfTheAssembly()
    {
        var provider = CreateProvider();

        var result = provider.GetServices<IBeforeCommitEventHandler>();

        result.Should().Contain(handler => handler is ClaimStateChangedReserveComponentHandler);
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void AddEvents_RegistersTheApprovalPostingHandler()
    {
        var provider = CreateProvider();

        var result = provider.GetServices<IAfterCommitEventHandler>();

        result.Should().ContainSingle(handler => handler is ReserveApprovedPostingHandler);
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void AddEvents_RegistersTheRetriedPostingHandler()
    {
        var provider = CreateProvider();

        var result = provider.GetServices<IAfterCommitEventHandler>();

        result.Should().ContainSingle(handler => handler is ReservePostingRetriedHandler);
    }

    [Fact]
    [Trait("Rule", "S:AUD-Events")]
    public void AddEvents_RegistersTheAuditHandlerOfAClaimStatusChange()
    {
        var provider = CreateProvider();

        var result = provider.GetServices<IBeforeCommitEventHandler>();

        result.Should().Contain(handler => handler is ClaimStateChangedAuditHandler);
    }

    [Fact]
    public void AddEvents_RegistersTheEventDispatcher()
    {
        var provider = CreateProvider();

        var result = provider.GetRequiredService<IEventDispatcher>();

        result.Should().BeOfType<EventDispatcher>();
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IReserveComponentRepository>());
        services.AddSingleton(Mock.Of<IPostingScheduler>());
        services.AddSingleton(Mock.Of<ICurrentUser>());
        services.AddSingleton(Mock.Of<IAuditLogService>());
        services.AddEvents(typeof(ApplicationServiceExtensions).Assembly);

        return services.BuildServiceProvider();
    }
}
