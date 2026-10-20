using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Application.ReserveComponents.History.Commands;
using ClaimsModule.Application.ReserveComponents.History.Dtos;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Application.Tests.Support.ReserveComponents;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.ReserveComponents.History.Commands;

public sealed class MarkReservePostedCommandTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL")]
    public async Task Handle_ForAnApprovedRow_MarksItPostedAndStoresTheJobId()
    {
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        var history = ApplicationReserves.Submit(component, claim, 5000m, ClaimHandlerContext.UserId);
        var context = new ClaimHandlerContext().WithComponent(component);
        var handler = CreateHandler(context);
        var command = new MarkReservePostedCommand(claim.Id, history.Id, "job-7");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().Be(new ReserveHistoryStatusDto(history.Id, "Pending", "Posted"));
        history.PostingDetails.JobId.Should().Be("job-7");
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    [Trait("Rule", "S:JOB-GL-Idempotent")]
    public async Task Handle_ForARowThatIsAlreadyPosted_SucceedsWithoutChangingOrCommitting()
    {
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        var history = ApplicationReserves.Submit(component, claim, 5000m, ClaimHandlerContext.UserId);
        var context = new ClaimHandlerContext().WithComponent(component);
        var handler = CreateHandler(context);
        await handler.Handle(new MarkReservePostedCommand(claim.Id, history.Id, "job-7"), CancellationToken.None);
        var command = new MarkReservePostedCommand(claim.Id, history.Id, "job-8");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().Be(new ReserveHistoryStatusDto(history.Id, "Posted", "Posted"));
        history.PostingDetails.JobId.Should().Be("job-7");
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_WithoutAJobId_ReturnsTheRuleErrorAndDoesNotCommit()
    {
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        var history = ApplicationReserves.Submit(component, claim, 5000m, ClaimHandlerContext.UserId);
        var context = new ClaimHandlerContext().WithComponent(component);
        var handler = CreateHandler(context);
        var command = new MarkReservePostedCommand(claim.Id, history.Id, string.Empty);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Select(error => error.Code).Should().Equal("RESERVE_POSTING_JOB_REQUIRED");
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenTheTransactionDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var historyId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutHistory(historyId);
        var handler = CreateHandler(context);
        var command = new MarkReservePostedCommand(Guid.NewGuid(), historyId, "job-7");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ReserveErrors.HistoryNotFound(historyId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_WithNullJobId_FailsOnTheField()
    {
        var validator = new MarkReservePostedCommandValidator();
        var command = new MarkReservePostedCommand(Guid.NewGuid(), Guid.NewGuid(), null!);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.JobId);
    }

    private static MarkReservePostedCommandHandler CreateHandler(ClaimHandlerContext context)
    {
        return new MarkReservePostedCommandHandler(context.ReserveComponents, context.UnitOfWork);
    }
}
