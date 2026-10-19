using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Application.ReserveComponents.History;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Application.Tests.Support.ReserveComponents;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.ReserveComponents.History;

public sealed class MarkReservePostingFailedCommandTests
{
    [Fact]
    [Trait("Rule", "S:JOB-GL-Failure")]
    public async Task Handle_ForAnApprovedRow_MarksItFailedWithTheReason()
    {
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        var history = ApplicationReserves.Submit(component, claim, 5000m, ClaimHandlerContext.UserId);
        var context = new ClaimHandlerContext().WithComponent(component);
        var handler = CreateHandler(context);
        var command = new MarkReservePostingFailedCommand(claim.Id, history.Id, "job-7", "Ledger unavailable");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().Be(new ReserveHistoryStatusDto(history.Id, "Pending", "Failed"));
        history.PostingDetails.FailureReason.Should().Be("Ledger unavailable");
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenTheTransactionDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var historyId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutHistory(historyId);
        var handler = CreateHandler(context);
        var command = new MarkReservePostingFailedCommand(Guid.NewGuid(), historyId, "job-7", "reason");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ReserveErrors.HistoryNotFound(historyId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_WithNullFailureReason_FailsOnTheField()
    {
        var validator = new MarkReservePostingFailedCommandValidator();
        var command = new MarkReservePostingFailedCommand(Guid.NewGuid(), Guid.NewGuid(), "job-7", null!);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.FailureReason);
    }

    private static MarkReservePostingFailedCommandHandler CreateHandler(ClaimHandlerContext context)
    {
        return new MarkReservePostingFailedCommandHandler(context.ReserveComponents, context.UnitOfWork);
    }
}
