using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Handlers;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.Claims.Parties.Events;

namespace ClaimsModule.Application.Tests.Audit.Handlers;

public sealed class PartyRemovedAuditHandlerTests
{
    [Fact]
    [Trait("Rule", "S:AUD-Events")]
    public async Task HandleAsync_WritesAPartyRemovedEntryForTheParty()
    {
        var log = new RecordingAuditLog();
        var handler = new PartyRemovedAuditHandler(log);
        var domainEvent = new PartyRemovedEvent(Guid.NewGuid(), Guid.NewGuid(), "Witness");

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        var entry = log.Entries.Should().ContainSingle().Subject;
        entry.EventType.Should().Be(AuditEventTypes.PartyRemoved);
        entry.RelatedEntityId.Should().Be(domainEvent.PartyId);
        entry.OldValue.Should().Contain("Witness");
    }
}
