using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Handlers;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.Claims.Events;

namespace ClaimsModule.Application.Tests.Audit.Handlers;

public sealed class ClaimCreatedAuditHandlerTests
{
    [Fact]
    [Trait("Rule", "S:AUD-Events")]
    public async Task HandleAsync_WritesAClaimCreatedEntry()
    {
        var log = new RecordingAuditLog();
        var handler = new ClaimCreatedAuditHandler(log);
        var domainEvent = new ClaimCreatedEvent(Guid.NewGuid(), "CLM-1");

        await handler.HandleAsync(domainEvent, EventContexts.For(domainEvent), CancellationToken.None);

        var entry = log.Entries.Should().ContainSingle().Subject;
        entry.EventType.Should().Be(AuditEventTypes.ClaimCreated);
        entry.ClaimId.Should().Be(domainEvent.ClaimId);
        entry.RelatedEntityId.Should().Be(domainEvent.ClaimId);
    }
}
