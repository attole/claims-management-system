using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Handlers;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.ReserveComponents.History.Events;

namespace ClaimsModule.Application.Tests.Audit.Handlers;

public sealed class ReservePostingAuditHandlerTests
{
    [Theory]
    [Trait("Rule", "S:AUD-Events")]
    [InlineData("Posted", AuditEventTypes.GlPostingSimulated)]
    [InlineData("Failed", AuditEventTypes.GlPostingFailed)]
    public async Task HandleAsync_WritesTheEntryOfTheNewPostingState(string target, string eventType)
    {
        var log = new RecordingAuditLog();
        var handler = new ReservePostingAuditHandler(log);
        var domainEvent = CreateEvent(target);

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        var entry = log.Entries.Should().ContainSingle().Subject;
        entry.EventType.Should().Be(eventType);
        entry.NewValue.Should().Contain("job-1").And.Contain("gateway down").And.Contain("2500");
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL")]
    public async Task HandleAsync_ForAPostedRow_DescribesTheDebitAndCreditJournal()
    {
        var log = new RecordingAuditLog();
        var handler = new ReservePostingAuditHandler(log);
        var domainEvent = CreateEvent("Posted");

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        var entry = log.Entries.Should().ContainSingle().Subject;
        entry.Description.Should().Contain("DR").And.Contain("CR").And.Contain("2500.00");
        entry.NewValue.Should().Contain("Outstanding Loss Reserves");
    }

    [Theory]
    [Trait("Rule", "S:AUD-Events")]
    [InlineData("Pending")]
    [InlineData("Cancelled")]
    public async Task HandleAsync_WhenTheStateIsNotOnTheList_WritesNothing(string target)
    {
        var log = new RecordingAuditLog();
        var handler = new ReservePostingAuditHandler(log);
        var domainEvent = CreateEvent(target);

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        log.Entries.Should().BeEmpty();
    }

    private static ReserveHistoryPostingStateChangedEvent CreateEvent(string target)
    {
        return new ReserveHistoryPostingStateChangedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Pending",
            target,
            "job-1",
            "gateway down",
            2500m);
    }
}
