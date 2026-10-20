using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Handlers;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.Claims.Issues.Events;

namespace ClaimsModule.Application.Tests.Audit.Handlers;

public sealed class ValidationIssueRaisedAuditHandlerTests
{
    [Fact]
    [Trait("Rule", "S:AUD-Events")]
    public async Task HandleAsync_WritesAValidationIssueAddedEntryForTheIssue()
    {
        var log = new RecordingAuditLog();
        var handler = new ValidationIssueRaisedAuditHandler(log);
        var domainEvent = new ValidationIssueRaisedEvent(Guid.NewGuid(), Guid.NewGuid(), "NO_PARTY", "Critical");

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        var entry = log.Entries.Should().ContainSingle().Subject;
        entry.EventType.Should().Be(AuditEventTypes.ValidationIssueAdded);
        entry.RelatedEntityId.Should().Be(domainEvent.IssueId);
    }
}
