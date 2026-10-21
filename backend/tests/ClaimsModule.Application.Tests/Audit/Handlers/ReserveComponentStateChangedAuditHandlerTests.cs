using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Handlers;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.ReserveComponents.Events;

namespace ClaimsModule.Application.Tests.Audit.Handlers;

public sealed class ReserveComponentStateChangedAuditHandlerTests
{
    [Fact]
    [Trait("Rule", "S:BR-A-02")]
    public async Task HandleAsync_WritesTheOldAndNewStateOfTheComponent()
    {
        var log = new RecordingAuditLog();
        var handler = new ReserveComponentStateChangedAuditHandler(log);
        var domainEvent = new ReserveComponentStateChangedEvent(Guid.NewGuid(), Guid.NewGuid(), "Active", "Closed");

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        var entry = log.Entries.Should().ContainSingle().Subject;
        entry.EventType.Should().Be(AuditEventTypes.Additional.ReserveComponentStateChanged);
        entry.OldValue.Should().Contain("Active");
        entry.NewValue.Should().Contain("Closed");
        entry.RelatedEntityId.Should().Be(domainEvent.ReserveComponentId);
    }
}
