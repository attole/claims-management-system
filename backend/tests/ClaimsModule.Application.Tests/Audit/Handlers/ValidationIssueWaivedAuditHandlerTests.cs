using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Handlers;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.Claims.Issues.Events;

namespace ClaimsModule.Application.Tests.Audit.Handlers;

public sealed class ValidationIssueWaivedAuditHandlerTests
{
    [Fact]
    [Trait("Rule", "S:BR-A-02")]
    public async Task HandleAsync_WritesAWaivedEntryForTheIssue()
    {
        var log = new RecordingAuditLog();
        var handler = new ValidationIssueWaivedAuditHandler(log);
        var domainEvent = new ValidationIssueWaivedEvent(Guid.NewGuid(), Guid.NewGuid(), "LOSS_DATE_FUTURE");

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        var entry = log.Entries.Should().ContainSingle().Subject;
        entry.EventType.Should().Be(AuditEventTypes.Additional.ValidationIssueWaived);
        entry.NewValue.Should().Contain("LOSS_DATE_FUTURE");
    }
}
