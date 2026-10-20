using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.Claims.Events;

namespace ClaimsModule.Application.Tests.Events.Contracts;

public sealed class IAfterCommitEventHandlerTests
{
    [Fact]
    public void EventType_GenericHandler_IsTheTypeOfItsEvent()
    {
        IAfterCommitEventHandler handler = new RecordingHandler();

        var result = handler.EventType;

        result.Should().Be(typeof(ClaimCreatedEvent));
    }

    [Fact]
    public async Task HandleAsync_ThroughTheNonGenericInterface_PassesTheTypedEventToTheHandler()
    {
        var recording = new RecordingHandler();
        IAfterCommitEventHandler handler = recording;
        var domainEvent = new ClaimCreatedEvent(Guid.NewGuid(), "CLM-1");
        var extendedEvent = ExtendedEvents.For(domainEvent);

        await handler.HandleAsync(extendedEvent, CancellationToken.None);

        recording.Events.Should().Equal(domainEvent);
        recording.ExtendedHandled.Should().Equal(extendedEvent);
    }

    private sealed class RecordingHandler : IAfterCommitEventHandler<ClaimCreatedEvent>
    {
        public List<ClaimCreatedEvent> Events { get; } = [];

        public List<ExtendedEvent> ExtendedHandled { get; } = [];

        public Task HandleAsync(
            ClaimCreatedEvent domainEvent,
            ExtendedEvent extendedEvent,
            CancellationToken cancellationToken)
        {
            Events.Add(domainEvent);
            ExtendedHandled.Add(extendedEvent);

            return Task.CompletedTask;
        }
    }
}
