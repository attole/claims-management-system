using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Handlers;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.ReserveComponents.History.Events;

namespace ClaimsModule.Application.Tests.Audit.Handlers;

public sealed class ReserveApprovalAuditHandlerTests
{
    [Theory]
    [Trait("Rule", "S:AUD-Events")]
    [InlineData("AutoApproved", AuditEventTypes.ReserveAutoApproved)]
    [InlineData("Approved", AuditEventTypes.ReserveApproved)]
    [InlineData("Rejected", AuditEventTypes.ReserveRejected)]
    [InlineData("Cancelled", AuditEventTypes.ReserveRetracted)]
    public async Task HandleAsync_WritesTheEntryOfTheNewApprovalState(string target, string eventType)
    {
        var log = new RecordingAuditLog();
        var handler = new ReserveApprovalAuditHandler(log);
        var domainEvent = CreateEvent(target);

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        var entry = log.Entries.Should().ContainSingle().Subject;
        entry.EventType.Should().Be(eventType);
        entry.RelatedEntityId.Should().Be(domainEvent.ReserveHistoryId);
    }

    [Fact]
    [Trait("Rule", "S:AUD-Events")]
    public async Task HandleAsync_WhenRejected_KeepsTheReasonInTheOldValue()
    {
        var log = new RecordingAuditLog();
        var handler = new ReserveApprovalAuditHandler(log);
        var domainEvent = CreateEvent("Rejected");

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        log.Entries.Single().OldValue.Should().Contain("Too high");
    }

    [Fact]
    [Trait("Rule", "S:AUD-Events")]
    public async Task HandleAsync_WhenApproved_KeepsTheReasonInTheNewValue()
    {
        var log = new RecordingAuditLog();
        var handler = new ReserveApprovalAuditHandler(log);
        var domainEvent = CreateEvent("Approved");

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        log.Entries.Single().NewValue.Should().Contain("Too high");
    }

    [Fact]
    [Trait("Rule", "S:AUD-Events")]
    public async Task HandleAsync_WhenTheStateIsNotOnTheList_WritesNothing()
    {
        var log = new RecordingAuditLog();
        var handler = new ReserveApprovalAuditHandler(log);
        var domainEvent = CreateEvent("PendingApproval");

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        log.Entries.Should().BeEmpty();
    }

    private static ReserveHistoryApprovalStateChangedEvent CreateEvent(string target)
    {
        return new ReserveHistoryApprovalStateChangedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "PendingApproval",
            target,
            "Too high");
    }
}
