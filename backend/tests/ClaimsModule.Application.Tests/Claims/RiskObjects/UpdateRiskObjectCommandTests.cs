using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.RiskObjects;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.RiskObjects;

public sealed class UpdateRiskObjectCommandTests
{
    [Fact]
    public async Task Handle_WhenTheRiskObjectExists_UpdatesItAndCommits()
    {
        var claim = ApplicationClaims.WithRiskObjects();
        var riskObject = claim.RiskObjects[0];
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = new UpdateRiskObjectCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.Mapper);
        var command = new UpdateRiskObjectCommand(claim.Id, riskObject.Id, ApplicationRequests.Garage());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AssetDescription.Should().Be("Garage");
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = new UpdateRiskObjectCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.Mapper);
        var command = new UpdateRiskObjectCommand(claimId, Guid.NewGuid(), ApplicationRequests.Garage());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenTheRiskObjectDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claim = ApplicationClaims.WithRiskObjects();
        var unknownId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = new UpdateRiskObjectCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.Mapper);
        var command = new UpdateRiskObjectCommand(claim.Id, unknownId, ApplicationRequests.Garage());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.RiskObjectNotFound(unknownId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_ForAValidCommand_HasNoErrors()
    {
        var validator = new UpdateRiskObjectCommandValidator();
        var command = new UpdateRiskObjectCommand(Guid.NewGuid(), Guid.NewGuid(), ApplicationRequests.Garage());

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnEmptyClaimId_FailsOnTheField()
    {
        var validator = new UpdateRiskObjectCommandValidator();
        var command = new UpdateRiskObjectCommand(Guid.Empty, Guid.NewGuid(), ApplicationRequests.Garage());

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.ClaimId);
    }

    [Fact]
    public void Validate_WithAnEmptyRiskObjectId_FailsOnTheField()
    {
        var validator = new UpdateRiskObjectCommandValidator();
        var command = new UpdateRiskObjectCommand(Guid.NewGuid(), Guid.Empty, ApplicationRequests.Garage());

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.RiskObjectId);
    }
}
