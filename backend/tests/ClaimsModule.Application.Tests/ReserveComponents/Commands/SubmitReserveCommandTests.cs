using ClaimsModule.Application.Claims;
using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Application.ReserveComponents.Commands;
using ClaimsModule.Application.ReserveComponents.Requests;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Application.Tests.Support.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.History;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.ReserveComponents.Commands;

public sealed class SubmitReserveCommandTests
{
    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public async Task Handle_ForANewComponentWithinTheAutoLimit_StoresItAutoApproved()
    {
        var claim = ApplicationClaims.Opened();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = CreateHandler(context);
        var command = new SubmitReserveCommand(claim.Id, NewComponentRequest(5000m));

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.ApprovalStatus.Should().Be("AutoApproved");
        context.AddedComponents.Should().ContainSingle();
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public async Task Handle_ForANewComponentAboveTheAutoLimit_StoresItPending()
    {
        var claim = ApplicationClaims.Opened();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = CreateHandler(context);
        var command = new SubmitReserveCommand(claim.Id, NewComponentRequest(20000m));

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.ApprovalStatus.Should().Be("PendingApproval");
        result.Value.TransactionType.Should().Be("Add");
    }

    [Fact]
    public async Task Handle_ForAnExistingComponent_AddsARowWithoutStoringAComponent()
    {
        var claim = ApplicationClaims.Opened();
        var component = ApplicationReserves.Component(claim);
        var context = new ClaimHandlerContext().WithClaim(claim).WithComponent(component);
        var handler = CreateHandler(context);
        var reserve = new ReserveRequest(ReserveTransactionType.Add, 1000m, "Adjustment")
        {
            ComponentId = component.Id,
        };
        var command = new SubmitReserveCommand(claim.Id, reserve);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.ReserveComponentId.Should().Be(component.Id);
        context.AddedComponents.Should().BeEmpty();
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_ForAnUnknownComponent_ReturnsNotFoundAndDoesNotCommit()
    {
        var claim = ApplicationClaims.Opened();
        var componentId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithClaim(claim).WithoutComponent(componentId);
        var handler = CreateHandler(context);
        var reserve = new ReserveRequest(ReserveTransactionType.Add, 1000m, "Adjustment")
        {
            ComponentId = componentId,
        };
        var command = new SubmitReserveCommand(claim.Id, reserve);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ReserveErrors.ComponentNotFound(componentId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    [Trait("Rule", "S:BR-R-01")]
    public async Task Handle_ForAZeroAmount_ReturnsTheRuleErrorAndStoresNothing()
    {
        var claim = ApplicationClaims.Opened();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = CreateHandler(context);
        var command = new SubmitReserveCommand(claim.Id, NewComponentRequest(0m));

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Select(error => error.Code).Should().Equal("RESERVE_AMOUNT_ZERO");
        context.AddedComponents.Should().BeEmpty();
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = CreateHandler(context);
        var command = new SubmitReserveCommand(claimId, NewComponentRequest(1000m));

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_ForAValidCommand_HasNoErrors()
    {
        var validator = new SubmitReserveCommandValidator();
        var command = new SubmitReserveCommand(Guid.NewGuid(), NewComponentRequest(1000m));

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnEmptyClaimId_FailsOnTheField()
    {
        var validator = new SubmitReserveCommandValidator();
        var command = new SubmitReserveCommand(Guid.Empty, NewComponentRequest(1000m));

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.ClaimId);
    }

    private static SubmitReserveCommandHandler CreateHandler(ClaimHandlerContext context)
    {
        return new SubmitReserveCommandHandler(
            context.Claims,
            context.ReserveComponents,
            context.Reserves,
            context.UnitOfWork,
            context.Clock,
            context.User,
            context.Mapper);
    }

    private static ReserveRequest NewComponentRequest(decimal amount)
    {
        return new ReserveRequest(ReserveTransactionType.Add, amount, "Initial estimate")
        {
            ComponentType = ReserveComponentTypeKind.Indemnity,
        };
    }
}
