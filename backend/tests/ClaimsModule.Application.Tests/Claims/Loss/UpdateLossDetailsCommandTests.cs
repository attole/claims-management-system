using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Loss;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.Loss;

public sealed class UpdateLossDetailsCommandTests
{
    [Fact]
    public async Task Handle_WhenTheClaimIsEditable_UpdatesTheLossAndCommits()
    {
        var claim = ApplicationClaims.Draft();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = new UpdateLossDetailsCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Mapper);
        var command = new UpdateLossDetailsCommand(claim.Id, ApplicationRequests.Loss());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CauseOfLossCode.Should().Be("WATER");
        result.Value.Description.Should().Be(ApplicationRequests.Loss().Description);
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = new UpdateLossDetailsCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Mapper);
        var command = new UpdateLossDetailsCommand(claimId, ApplicationRequests.Loss());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_ForAValidCommand_HasNoErrors()
    {
        var validator = new UpdateLossDetailsCommandValidator();
        var command = new UpdateLossDetailsCommand(Guid.NewGuid(), ApplicationRequests.Loss());

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnEmptyClaimId_FailsOnTheField()
    {
        var validator = new UpdateLossDetailsCommandValidator();
        var command = new UpdateLossDetailsCommand(Guid.Empty, ApplicationRequests.Loss());

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.ClaimId);
    }

    [Fact]
    public void Validate_WithANegativeEstimate_FailsOnTheField()
    {
        var validator = new UpdateLossDetailsCommandValidator();
        var command = new UpdateLossDetailsCommand(
            Guid.NewGuid(),
            ApplicationRequests.Loss() with { EstimatedLossAmount = -1m });

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.Loss.EstimatedLossAmount);
    }
}
