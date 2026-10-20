using ClaimsModule.Application.ReserveComponents.History.Handlers;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.ReserveComponents.History.Events;

namespace ClaimsModule.Application.Tests.ReserveComponents.History.Handlers;

public sealed class ReserveApprovedPostingHandlerTests
{
    [Theory]
    [Trait("Rule", "S:RES-PendingFlow")]
    [InlineData("Approved")]
    [InlineData("AutoApproved")]
    public async Task HandleAsync_WhenTheRowIsApproved_EnqueuesThePostingJob(string target)
    {
        var context = new ClaimHandlerContext();
        var handler = new ReserveApprovedPostingHandler(context.Scheduler, context.User);
        var domainEvent = CreateEvent("PendingApproval", target);

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        context.VerifyEnqueued(Times.Once());
    }

    [Theory]
    [InlineData("Rejected")]
    [InlineData("Cancelled")]
    public async Task HandleAsync_WhenTheRowIsRefused_EnqueuesNothing(string target)
    {
        var context = new ClaimHandlerContext();
        var handler = new ReserveApprovedPostingHandler(context.Scheduler, context.User);
        var domainEvent = CreateEvent("PendingApproval", target);

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        context.VerifyEnqueued(Times.Never());
    }

    private static ReserveHistoryApprovalStateChangedEvent CreateEvent(string from, string to)
    {
        return new ReserveHistoryApprovalStateChangedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            from,
            to,
            string.Empty);
    }
}
