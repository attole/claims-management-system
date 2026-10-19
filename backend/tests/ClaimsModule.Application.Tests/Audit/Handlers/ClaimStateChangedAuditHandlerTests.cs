using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Handlers;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.Claims.Events;
using ClaimsModule.Domain.Claims.StateChanges;

namespace ClaimsModule.Application.Tests.Audit.Handlers;

public sealed class ClaimStateChangedAuditHandlerTests
{
    [Fact]
    [Trait("Rule", "S:AUD-Events")]
    public async Task HandleAsync_WritesAStatusChangedEntryWithBothStatuses()
    {
        var log = new RecordingAuditLog();
        var handler = new ClaimStateChangedAuditHandler(log);
        var domainEvent = CreateEvent("Draft", "Open");

        await handler.HandleAsync(domainEvent, EventContexts.For(domainEvent), CancellationToken.None);

        var entry = log.Entries.Should().ContainSingle().Subject;
        entry.EventType.Should().Be(AuditEventTypes.StatusChanged);
        entry.OldValue.Should().Contain("Draft");
        entry.NewValue.Should().Contain("Open");
    }

    [Theory]
    [Trait("Rule", "S:AUD-Events")]
    [InlineData("Closed", AuditEventTypes.ClaimClosed)]
    [InlineData("Reopened", AuditEventTypes.ClaimReopened)]
    public async Task HandleAsync_WhenTheClaimIsClosedOrReopened_AlsoWritesTheSpecificEntry(
        string target,
        string eventType)
    {
        var log = new RecordingAuditLog();
        var handler = new ClaimStateChangedAuditHandler(log);
        var domainEvent = CreateEvent("Open", target);

        await handler.HandleAsync(domainEvent, EventContexts.For(domainEvent), CancellationToken.None);

        log.Entries.Select(entry => entry.EventType).Should().Equal(AuditEventTypes.StatusChanged, eventType);
        log.Entries.Last().NewValue.Should().Contain("Duplicate");
    }

    private static ClaimStateChangedEvent CreateEvent(string from, string to)
    {
        var reason = new StateChangeReason("DUP", "Duplicate");

        return new ClaimStateChangedEvent(Guid.NewGuid(), from, to, reason);
    }
}
