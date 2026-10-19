using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Application.ReserveComponents.History;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Application.Tests.Support.ReserveComponents;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.ReserveComponents.History;

public sealed class RetractReserveCommandTests
{
    [Fact]
    [Trait("Rule", "S:API-POST-reserve-retract")]
    public async Task Handle_ByTheSubmitter_RetractsAndCommits()
    {
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        var history = ApplicationReserves.Submit(component, claim, 20000m, ClaimHandlerContext.UserId);
        var context = new ClaimHandlerContext().WithComponent(component);
        var handler = CreateHandler(context);
        var command = new RetractReserveCommand(claim.Id, history.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().Be(new ReserveHistoryStatusDto(history.Id, "PendingApproval", "Cancelled"));
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    [Trait("Rule", "S:API-POST-reserve-retract")]
    public async Task Handle_ByAnotherUser_ReturnsTheRuleErrorAndDoesNotCommit()
    {
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        var history = ApplicationReserves.Submit(component, claim, 20000m, ApplicationReserves.Submitter);
        var context = new ClaimHandlerContext().WithComponent(component);
        var handler = CreateHandler(context);
        var command = new RetractReserveCommand(claim.Id, history.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Select(error => error.Code).Should().Equal("S:API-POST-reserve-retract");
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenTheTransactionDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var historyId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutHistory(historyId);
        var handler = CreateHandler(context);
        var command = new RetractReserveCommand(Guid.NewGuid(), historyId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ReserveErrors.HistoryNotFound(historyId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_WithAnEmptyId_FailsOnTheField()
    {
        var validator = new RetractReserveCommandValidator();
        var command = new RetractReserveCommand(Guid.NewGuid(), Guid.Empty);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.ReserveHistoryId);
    }

    private static RetractReserveCommandHandler CreateHandler(ClaimHandlerContext context)
    {
        return new RetractReserveCommandHandler(
            context.ReserveComponents,
            context.UnitOfWork,
            context.Clock,
            context.User);
    }
}
