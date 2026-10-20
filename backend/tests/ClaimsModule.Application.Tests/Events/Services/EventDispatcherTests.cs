using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Application.Events.Services;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.Claims.Events;
using ClaimsModule.Domain.Claims.Parties.Events;

namespace ClaimsModule.Application.Tests.Events.Services;

public sealed class EventDispatcherTests
{
    [Fact]
    public async Task DispatchBeforeCommitAsync_CallsEveryBeforeCommitHandlerOfTheEventType()
    {
        var first = CreateBeforeCommitHandler(typeof(ClaimCreatedEvent));
        var second = CreateBeforeCommitHandler(typeof(ClaimCreatedEvent));
        var dispatcher = new EventDispatcher([first.Object, second.Object], []);
        var extendedEvent = ExtendedEvents.For(new ClaimCreatedEvent(Guid.NewGuid(), "CLM-1"));

        await dispatcher.DispatchBeforeCommitAsync([extendedEvent], CancellationToken.None);

        first.Verify(mock => mock.HandleAsync(extendedEvent, CancellationToken.None), Times.Once());
        second.Verify(mock => mock.HandleAsync(extendedEvent, CancellationToken.None), Times.Once());
    }

    [Fact]
    public async Task DispatchBeforeCommitAsync_HandlerOfAnotherEventType_IsNotCalled()
    {
        var handler = CreateBeforeCommitHandler(typeof(PartyAddedEvent));
        var dispatcher = new EventDispatcher([handler.Object], []);
        var extendedEvent = ExtendedEvents.For(new ClaimCreatedEvent(Guid.NewGuid(), "CLM-1"));

        await dispatcher.DispatchBeforeCommitAsync([extendedEvent], CancellationToken.None);

        handler.Verify(
            mock => mock.HandleAsync(It.IsAny<ExtendedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never());
    }

    [Fact]
    public async Task DispatchBeforeCommitAsync_CallsTheHandlersOnceForEachEvent()
    {
        var handler = CreateBeforeCommitHandler(typeof(ClaimCreatedEvent));
        var dispatcher = new EventDispatcher([handler.Object], []);
        var first = ExtendedEvents.For(new ClaimCreatedEvent(Guid.NewGuid(), "CLM-1"));
        var second = ExtendedEvents.For(new ClaimCreatedEvent(Guid.NewGuid(), "CLM-2"));

        await dispatcher.DispatchBeforeCommitAsync([first, second], CancellationToken.None);

        handler.Verify(mock => mock.HandleAsync(first, CancellationToken.None), Times.Once());
        handler.Verify(mock => mock.HandleAsync(second, CancellationToken.None), Times.Once());
    }

    [Fact]
    public async Task DispatchBeforeCommitAsync_DoesNotCallAfterCommitHandlers()
    {
        var handler = CreateAfterCommitHandler(typeof(ClaimCreatedEvent));
        var dispatcher = new EventDispatcher([], [handler.Object]);
        var extendedEvent = ExtendedEvents.For(new ClaimCreatedEvent(Guid.NewGuid(), "CLM-1"));

        await dispatcher.DispatchBeforeCommitAsync([extendedEvent], CancellationToken.None);

        handler.Verify(
            mock => mock.HandleAsync(It.IsAny<ExtendedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never());
    }

    [Fact]
    public async Task DispatchAfterCommitAsync_CallsTheAfterCommitHandlersOfTheEventType()
    {
        var handler = CreateAfterCommitHandler(typeof(ClaimCreatedEvent));
        var dispatcher = new EventDispatcher([], [handler.Object]);
        var extendedEvent = ExtendedEvents.For(new ClaimCreatedEvent(Guid.NewGuid(), "CLM-1"));

        await dispatcher.DispatchAfterCommitAsync([extendedEvent], CancellationToken.None);

        handler.Verify(mock => mock.HandleAsync(extendedEvent, CancellationToken.None), Times.Once());
    }

    [Fact]
    public async Task DispatchBeforeCommitAsync_WhenAHandlerThrows_ThrowsTheOriginalException()
    {
        var handler = CreateBeforeCommitHandler(typeof(ClaimCreatedEvent));
        handler
            .Setup(mock => mock.HandleAsync(It.IsAny<ExtendedEvent>(), default))
            .ThrowsAsync(new InvalidOperationException("refused"));
        var dispatcher = new EventDispatcher([handler.Object], []);
        var extendedEvent = ExtendedEvents.For(new ClaimCreatedEvent(Guid.NewGuid(), "CLM-1"));

        Func<Task> act = () => dispatcher.DispatchBeforeCommitAsync([extendedEvent], CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("refused");
    }

    [Fact]
    public async Task DispatchBeforeCommitAsync_WhenNoHandlerIsRegistered_DoesNothing()
    {
        var dispatcher = new EventDispatcher([], []);
        var extendedEvent = ExtendedEvents.For(new ClaimCreatedEvent(Guid.NewGuid(), "CLM-1"));

        Func<Task> act = () => dispatcher.DispatchBeforeCommitAsync([extendedEvent], CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    private static Mock<IBeforeCommitEventHandler> CreateBeforeCommitHandler(Type eventType)
    {
        var handler = new Mock<IBeforeCommitEventHandler>();
        handler.SetupGet(mock => mock.EventType).Returns(eventType);

        return handler;
    }

    private static Mock<IAfterCommitEventHandler> CreateAfterCommitHandler(Type eventType)
    {
        var handler = new Mock<IAfterCommitEventHandler>();
        handler.SetupGet(mock => mock.EventType).Returns(eventType);

        return handler;
    }
}
