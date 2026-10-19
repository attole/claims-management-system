using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Application.Events.Services;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.Claims.Events;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Application.Tests.Events.Services;

public sealed class EventDispatcherTests
{
    [Fact]
    public async Task DispatchBeforeCommitAsync_CallsTheHandlersOfTheEventType()
    {
        var handler = new Mock<IBeforeCommitEventHandler<ClaimCreatedEvent>>();
        var provider = CreateProvider(handler.Object);
        var dispatcher = new EventDispatcher(provider);
        var domainEvent = new ClaimCreatedEvent(Guid.NewGuid(), "CLM-1");
        var context = EventContexts.For(domainEvent);

        await dispatcher.DispatchBeforeCommitAsync([context], CancellationToken.None);

        handler.Verify(
            mock => mock.HandleAsync(domainEvent, context, CancellationToken.None),
            Times.Once());
    }

    [Fact]
    public async Task DispatchBeforeCommitAsync_DoesNotCallAfterCommitHandlers()
    {
        var handler = new Mock<IAfterCommitEventHandler<ClaimCreatedEvent>>();
        var provider = CreateProvider(handler.Object);
        var dispatcher = new EventDispatcher(provider);
        var domainEvent = new ClaimCreatedEvent(Guid.NewGuid(), "CLM-1");
        var context = EventContexts.For(domainEvent);

        await dispatcher.DispatchBeforeCommitAsync([context], CancellationToken.None);

        handler.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DispatchAfterCommitAsync_CallsTheAfterCommitHandlers()
    {
        var handler = new Mock<IAfterCommitEventHandler<ClaimCreatedEvent>>();
        var provider = CreateProvider(handler.Object);
        var dispatcher = new EventDispatcher(provider);
        var domainEvent = new ClaimCreatedEvent(Guid.NewGuid(), "CLM-1");
        var context = EventContexts.For(domainEvent);

        await dispatcher.DispatchAfterCommitAsync([context], CancellationToken.None);

        handler.Verify(
            mock => mock.HandleAsync(domainEvent, context, CancellationToken.None),
            Times.Once());
    }

    [Fact]
    public async Task DispatchBeforeCommitAsync_WhenAHandlerThrows_ThrowsTheOriginalException()
    {
        var handler = new Mock<IBeforeCommitEventHandler<ClaimCreatedEvent>>();
        handler
            .Setup(mock => mock.HandleAsync(It.IsAny<ClaimCreatedEvent>(), It.IsAny<EventContext>(), default))
            .ThrowsAsync(new InvalidOperationException("refused"));
        var dispatcher = new EventDispatcher(CreateProvider(handler.Object));
        var domainEvent = new ClaimCreatedEvent(Guid.NewGuid(), "CLM-1");
        var context = EventContexts.For(domainEvent);

        Func<Task> act = () => dispatcher.DispatchBeforeCommitAsync([context], CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("refused");
    }

    [Fact]
    public async Task DispatchBeforeCommitAsync_WhenNoHandlerIsRegistered_DoesNothing()
    {
        var dispatcher = new EventDispatcher(new ServiceCollection().BuildServiceProvider());
        var domainEvent = new ClaimCreatedEvent(Guid.NewGuid(), "CLM-1");
        var context = EventContexts.For(domainEvent);

        Func<Task> act = () => dispatcher.DispatchBeforeCommitAsync([context], CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    private static ServiceProvider CreateProvider<THandler>(THandler handler)
        where THandler : class
    {
        var services = new ServiceCollection();
        services.AddSingleton(handler);

        return services.BuildServiceProvider();
    }
}
