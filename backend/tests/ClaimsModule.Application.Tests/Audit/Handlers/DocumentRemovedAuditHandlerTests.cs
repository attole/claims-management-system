using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Handlers;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.ClaimDocuments.Events;

namespace ClaimsModule.Application.Tests.Audit.Handlers;

public sealed class DocumentRemovedAuditHandlerTests
{
    [Fact]
    [Trait("Rule", "S:BR-A-02")]
    public async Task HandleAsync_WritesARemovedEntryReferencingTheDocument()
    {
        var log = new RecordingAuditLog();
        var handler = new DocumentRemovedAuditHandler(log);
        var domainEvent = new ClaimDocumentRemovedEvent(Guid.NewGuid(), Guid.NewGuid());

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        var entry = log.Entries.Should().ContainSingle().Subject;
        entry.EventType.Should().Be(AuditEventTypes.Additional.DocumentRemoved);
        entry.RelatedEntityId.Should().Be(domainEvent.ClaimDocumentId);
    }
}
