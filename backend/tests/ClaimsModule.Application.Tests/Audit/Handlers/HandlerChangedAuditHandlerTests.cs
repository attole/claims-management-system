using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Handlers;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.Claims.Events;

namespace ClaimsModule.Application.Tests.Audit.Handlers;

public sealed class HandlerChangedAuditHandlerTests
{
    [Fact]
    [Trait("Rule", "S:BR-A-02")]
    public async Task HandleAsync_WhenTheHandlerChanged_WritesTheOldAndNewHandler()
    {
        var log = new RecordingAuditLog();
        var handler = new HandlerChangedAuditHandler(log);
        var oldHandlerId = Guid.NewGuid();
        var newHandlerId = Guid.NewGuid();
        var domainEvent = new ClaimHandlerChangedEvent(Guid.NewGuid(), oldHandlerId, newHandlerId);

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        var entry = log.Entries.Should().ContainSingle().Subject;
        entry.EventType.Should().Be(AuditEventTypes.Additional.HandlerChanged);
        entry.OldValue.Should().Contain(oldHandlerId.ToString());
        entry.NewValue.Should().Contain(newHandlerId.ToString());
    }

    [Fact]
    [Trait("Rule", "S:BR-A-02")]
    public async Task HandleAsync_WhenTheHandlerIsAssignedForTheFirstTime_WritesNothing()
    {
        var log = new RecordingAuditLog();
        var handler = new HandlerChangedAuditHandler(log);
        var domainEvent = new ClaimHandlerChangedEvent(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        log.Entries.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:BR-A-02")]
    public async Task HandleAsync_WhenTheHandlerDidNotChange_WritesNothing()
    {
        var log = new RecordingAuditLog();
        var handler = new HandlerChangedAuditHandler(log);
        var sameHandlerId = Guid.NewGuid();
        var domainEvent = new ClaimHandlerChangedEvent(Guid.NewGuid(), sameHandlerId, sameHandlerId);

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        log.Entries.Should().BeEmpty();
    }
}
