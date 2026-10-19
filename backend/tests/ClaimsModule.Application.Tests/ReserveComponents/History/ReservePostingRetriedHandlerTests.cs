using ClaimsModule.Application.Events;
using ClaimsModule.Application.ReserveComponents.History;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Domain.ReserveComponents.History.Events;

namespace ClaimsModule.Application.Tests.ReserveComponents.History;

public sealed class ReservePostingRetriedHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenAFailedPostingReturnsToPending_EnqueuesTheJob()
    {
        var context = new ClaimHandlerContext();
        var handler = new ReservePostingRetriedHandler(context.Scheduler);
        var domainEvent = CreateEvent("Failed", "Pending");

        await handler.HandleAsync(domainEvent, CreateContext(domainEvent), CancellationToken.None);

        context.VerifyEnqueuedWithJobId("retry-token");
    }

    [Theory]
    [InlineData("Pending", "Posted")]
    [InlineData("Pending", "Failed")]
    [InlineData("Pending", "Cancelled")]
    public async Task HandleAsync_ForAnyOtherPostingMove_EnqueuesNothing(string from, string to)
    {
        var context = new ClaimHandlerContext();
        var handler = new ReservePostingRetriedHandler(context.Scheduler);
        var domainEvent = CreateEvent(from, to);

        await handler.HandleAsync(domainEvent, CreateContext(domainEvent), CancellationToken.None);

        context.VerifyEnqueued(Times.Never());
    }

    private static ReserveHistoryPostingStateChangedEvent CreateEvent(string from, string to)
    {
        return new ReserveHistoryPostingStateChangedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            from,
            to,
            "retry-token",
            string.Empty);
    }

    private static EventContext CreateContext(ReserveHistoryPostingStateChangedEvent domainEvent)
    {
        return new EventContext(
            Guid.NewGuid(),
            ClaimHandlerContext.Now,
            ClaimHandlerContext.UserId,
            Guid.NewGuid(),
            domainEvent);
    }
}
