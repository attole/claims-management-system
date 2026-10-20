using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Commands;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.Commands;

public sealed class AssignHandlerCommandTests
{
    [Fact]
    public async Task Handle_WhenTheClaimIsEditable_AssignsTheHandlerAndCommits()
    {
        var claim = ApplicationClaims.Draft();
        var handlerId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = new AssignHandlerCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.Mapper);
        var command = new AssignHandlerCommand(claim.Id, handlerId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AssignedHandlerId.Should().Be(handlerId);
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = new AssignHandlerCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.Mapper);
        var command = new AssignHandlerCommand(claimId, Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_ForAValidCommand_HasNoErrors()
    {
        var validator = new AssignHandlerCommandValidator();
        var command = new AssignHandlerCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnEmptyClaimId_FailsOnTheField()
    {
        var validator = new AssignHandlerCommandValidator();
        var command = new AssignHandlerCommand(Guid.Empty, Guid.NewGuid());

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.ClaimId);
    }

    [Fact]
    public void Validate_WithAnEmptyHandlerId_FailsOnTheField()
    {
        var validator = new AssignHandlerCommandValidator();
        var command = new AssignHandlerCommand(Guid.NewGuid(), Guid.Empty);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.HandlerId);
    }
}
