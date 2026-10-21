using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Handlers;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.Claims.Issues.Events;

namespace ClaimsModule.Application.Tests.Audit.Handlers;

public sealed class ValidationIssueAcknowledgedAuditHandlerTests
{
    [Fact]
    [Trait("Rule", "S:BR-A-02")]
    public async Task HandleAsync_WritesAnAcknowledgedEntryForTheIssue()
    {
        var log = new RecordingAuditLog();
        var handler = new ValidationIssueAcknowledgedAuditHandler(log);
        var domainEvent = new ValidationIssueAcknowledgedEvent(Guid.NewGuid(), Guid.NewGuid(), "POLICY_PERIOD");

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        var entry = log.Entries.Should().ContainSingle().Subject;
        entry.EventType.Should().Be(AuditEventTypes.Additional.ValidationIssueAcknowledged);
        entry.RelatedEntityId.Should().Be(domainEvent.IssueId);
    }
}
