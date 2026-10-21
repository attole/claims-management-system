using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Handlers;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.Claims.Events;

namespace ClaimsModule.Application.Tests.Audit.Handlers;

public sealed class LimitOverrideChangedAuditHandlerTests
{
    [Theory]
    [Trait("Rule", "S:BR-A-02")]
    [InlineData(false, true, "set")]
    [InlineData(true, false, "cleared")]
    public async Task HandleAsync_WhenTheOverrideChanged_WritesTheOldAndNewState(bool was, bool isNow, string word)
    {
        var log = new RecordingAuditLog();
        var handler = new LimitOverrideChangedAuditHandler(log);
        var domainEvent = new AggregateLimitOverrideChangedEvent(Guid.NewGuid(), was, isNow);

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        var entry = log.Entries.Should().ContainSingle().Subject;
        entry.EventType.Should().Be(AuditEventTypes.Additional.LimitOverrideChanged);
        entry.Description.Should().Contain(word);
        entry.OldValue.Should().Contain(was ? "true" : "false");
    }

    [Theory]
    [Trait("Rule", "S:BR-A-02")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleAsync_WhenTheOverrideDidNotChange_WritesNothing(bool state)
    {
        var log = new RecordingAuditLog();
        var handler = new LimitOverrideChangedAuditHandler(log);
        var domainEvent = new AggregateLimitOverrideChangedEvent(Guid.NewGuid(), state, state);

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        log.Entries.Should().BeEmpty();
    }
}
