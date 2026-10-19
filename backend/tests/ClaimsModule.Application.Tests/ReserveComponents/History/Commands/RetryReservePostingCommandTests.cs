using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Application.ReserveComponents.History.Commands;
using ClaimsModule.Application.ReserveComponents.History.Dtos;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Application.Tests.Support.ReserveComponents;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.ReserveComponents.History.Commands;

public sealed class RetryReservePostingCommandTests
{
    [Fact]
    [Trait("Rule", "S:UI-Reserves")]
    public async Task Handle_ForAFailedPosting_ReturnsItToPendingWithANewJobId()
    {
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        var history = ApplicationReserves.Submit(component, claim, 5000m, ClaimHandlerContext.UserId);
        ApplicationReserves.FailPosting(component, history);
        var context = new ClaimHandlerContext().WithComponent(component);
        var handler = CreateHandler(context);
        var command = new RetryReservePostingCommand(claim.Id, history.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().Be(new ReserveHistoryStatusDto(history.Id, "Failed", "Pending"));
        history.PostingDetails.JobId.Should().NotBeEmpty().And.NotBe("job-1");
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_ForAPostingThatDidNotFail_ReturnsTheErrorAndDoesNotCommit()
    {
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        var history = ApplicationReserves.Submit(component, claim, 5000m, ClaimHandlerContext.UserId);
        var context = new ClaimHandlerContext().WithComponent(component);
        var handler = CreateHandler(context);
        var command = new RetryReservePostingCommand(claim.Id, history.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenTheTransactionDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var historyId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutHistory(historyId);
        var handler = CreateHandler(context);
        var command = new RetryReservePostingCommand(Guid.NewGuid(), historyId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ReserveErrors.HistoryNotFound(historyId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_WithAnEmptyId_FailsOnTheField()
    {
        var validator = new RetryReservePostingCommandValidator();
        var command = new RetryReservePostingCommand(Guid.Empty, Guid.NewGuid());

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.ClaimId);
    }

    private static RetryReservePostingCommandHandler CreateHandler(ClaimHandlerContext context)
    {
        return new RetryReservePostingCommandHandler(context.ReserveComponents, context.UnitOfWork);
    }
}
