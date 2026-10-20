using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Handlers;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.Claims.Parties.Events;

namespace ClaimsModule.Application.Tests.Audit.Handlers;

public sealed class PartyAddedAuditHandlerTests
{
    [Fact]
    [Trait("Rule", "S:AUD-Events")]
    public async Task HandleAsync_WritesAPartyAddedEntryForTheParty()
    {
        var log = new RecordingAuditLog();
        var handler = new PartyAddedAuditHandler(log);
        var domainEvent = new PartyAddedEvent(Guid.NewGuid(), Guid.NewGuid(), "Claimant");

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        var entry = log.Entries.Should().ContainSingle().Subject;
        entry.EventType.Should().Be(AuditEventTypes.PartyAdded);
        entry.RelatedEntityId.Should().Be(domainEvent.PartyId);
        entry.NewValue.Should().Contain("Claimant");
    }
}
