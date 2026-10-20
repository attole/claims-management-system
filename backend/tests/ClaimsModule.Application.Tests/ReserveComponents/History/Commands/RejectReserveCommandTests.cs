using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Application.ReserveComponents.History.Commands;
using ClaimsModule.Application.ReserveComponents.History.Dtos;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Application.Tests.Support.ReserveComponents;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.ReserveComponents.History.Commands;

public sealed class RejectReserveCommandTests
{
    [Fact]
    [Trait("Rule", "S:API-POST-reserve-reject")]
    public async Task Handle_WithAReason_RejectsAndCommits()
    {
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        var history = ApplicationReserves.Submit(component, claim, 20000m, ApplicationReserves.Submitter);
        var context = new ClaimHandlerContext().WithComponent(component);
        var handler = CreateHandler(context);
        var command = new RejectReserveCommand(claim.Id, history.Id, "Amount too high");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().Be(new ReserveHistoryStatusDto(history.Id, "PendingApproval", "Rejected"));
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_WithoutAReason_ReturnsTheRuleErrorAndDoesNotCommit()
    {
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        var history = ApplicationReserves.Submit(component, claim, 20000m, ApplicationReserves.Submitter);
        var context = new ClaimHandlerContext().WithComponent(component);
        var handler = CreateHandler(context);
        var command = new RejectReserveCommand(claim.Id, history.Id, string.Empty);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Select(error => error.Code).Should().Equal("RESERVE_REJECTION_REASON_REQUIRED");
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenTheTransactionDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var historyId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutHistory(historyId);
        var handler = CreateHandler(context);
        var command = new RejectReserveCommand(Guid.NewGuid(), historyId, "reason");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ReserveErrors.HistoryNotFound(historyId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_WithAnEmptyId_FailsOnTheField()
    {
        var validator = new RejectReserveCommandValidator();
        var command = new RejectReserveCommand(Guid.Empty, Guid.NewGuid(), "reason");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.ClaimId);
    }

    [Fact]
    public void Validate_WithNullReason_FailsOnTheField()
    {
        var validator = new RejectReserveCommandValidator();
        var command = new RejectReserveCommand(Guid.NewGuid(), Guid.NewGuid(), null!);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.Reason);
    }

    [Fact]
    public void Validate_WithAReasonLongerThanTheColumn_FailsOnTheField()
    {
        var validator = new RejectReserveCommandValidator();
        var reason = new string('x', ReserveFieldLengths.ReviewReason + 1);
        var command = new RejectReserveCommand(Guid.NewGuid(), Guid.NewGuid(), reason);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.Reason);
    }

    private static RejectReserveCommandHandler CreateHandler(ClaimHandlerContext context)
    {
        return new RejectReserveCommandHandler(
            context.ReserveComponents,
            context.UnitOfWork,
            context.Clock,
            context.User);
    }
}
