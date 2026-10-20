using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Handlers;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.ReserveComponents.History.Events;

namespace ClaimsModule.Application.Tests.Audit.Handlers;

public sealed class ReserveSubmittedAuditHandlerTests
{
    [Fact]
    [Trait("Rule", "S:AUD-Events")]
    public async Task HandleAsync_WritesAReserveCreatedEntryForTheHistoryRow()
    {
        var log = new RecordingAuditLog();
        var handler = new ReserveSubmittedAuditHandler(log);
        var domainEvent = new ReserveHistorySubmittedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 2500m);

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        var entry = log.Entries.Should().ContainSingle().Subject;
        entry.EventType.Should().Be(AuditEventTypes.ReserveCreated);
        entry.RelatedEntityId.Should().Be(domainEvent.ReserveHistoryId);
        entry.NewValue.Should().Contain("2500");
    }
}
