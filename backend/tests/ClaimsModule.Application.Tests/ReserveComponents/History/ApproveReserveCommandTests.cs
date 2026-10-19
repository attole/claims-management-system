using ClaimsModule.Application.Claims;
using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Application.ReserveComponents.History;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Application.Tests.Support.ReserveComponents;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.ReserveComponents.History;

public sealed class ApproveReserveCommandTests
{
    [Fact]
    [Trait("Rule", "S:API-POST-reserve-approve")]
    public async Task Handle_ByAManagerForAnotherUsersRow_ApprovesAndCommits()
    {
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        var history = ApplicationReserves.Submit(component, claim, 20000m, ApplicationReserves.Submitter);
        var context = new ClaimHandlerContext().WithClaim(claim).WithComponent(component);
        var handler = CreateHandler(context);
        var command = new ApproveReserveCommand(claim.Id, history.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().Be(new ReserveHistoryStatusDto(history.Id, "PendingApproval", "Approved"));
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    [Trait("Rule", "S:BR-R-03")]
    public async Task Handle_ForTheUsersOwnRow_ReturnsTheSelfApprovalErrorAndDoesNotCommit()
    {
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        var history = ApplicationReserves.Submit(component, claim, 20000m, ClaimHandlerContext.UserId);
        var context = new ClaimHandlerContext().WithClaim(claim).WithComponent(component);
        var handler = CreateHandler(context);
        var command = new ApproveReserveCommand(claim.Id, history.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Select(error => error.Code).Should().Equal("S:BR-R-03");
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = CreateHandler(context);
        var command = new ApproveReserveCommand(claimId, Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenTheTransactionDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claim = ApplicationClaims.Opened();
        var historyId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithClaim(claim).WithoutHistory(historyId);
        var handler = CreateHandler(context);
        var command = new ApproveReserveCommand(claim.Id, historyId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ReserveErrors.HistoryNotFound(historyId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_WithAnEmptyId_FailsOnTheField()
    {
        var validator = new ApproveReserveCommandValidator();
        var command = new ApproveReserveCommand(Guid.NewGuid(), Guid.Empty);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.ReserveHistoryId);
    }

    [Fact]
    public void Validate_ForValidIds_HasNoErrors()
    {
        var validator = new ApproveReserveCommandValidator();
        var command = new ApproveReserveCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static ApproveReserveCommandHandler CreateHandler(ClaimHandlerContext context)
    {
        return new ApproveReserveCommandHandler(
            context.Claims,
            context.ReserveComponents,
            context.Reserves,
            context.UnitOfWork,
            context.Clock,
            context.User);
    }
}
