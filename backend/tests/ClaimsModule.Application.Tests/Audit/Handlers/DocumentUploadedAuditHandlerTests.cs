using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Handlers;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.ClaimDocuments.Events;

namespace ClaimsModule.Application.Tests.Audit.Handlers;

public sealed class DocumentUploadedAuditHandlerTests
{
    [Fact]
    [Trait("Rule", "S:AUD-Events")]
    public async Task HandleAsync_WritesADocumentUploadedEntryForTheDocument()
    {
        var log = new RecordingAuditLog();
        var handler = new DocumentUploadedAuditHandler(log);
        var domainEvent = new ClaimDocumentUploadedEvent(Guid.NewGuid(), Guid.NewGuid(), "Report", "report.pdf");

        await handler.HandleAsync(domainEvent, EventContexts.For(domainEvent), CancellationToken.None);

        var entry = log.Entries.Should().ContainSingle().Subject;
        entry.EventType.Should().Be(AuditEventTypes.DocumentUploaded);
        entry.RelatedEntityId.Should().Be(domainEvent.ClaimDocumentId);
    }
}
