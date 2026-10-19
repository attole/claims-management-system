using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.RiskObjects;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Domain.Claims.RiskObjects;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.RiskObjects;

public sealed class AddRiskObjectCommandTests
{
    [Fact]
    public async Task Handle_WhenTheClaimIsEditable_AddsTheRiskObjectAndCommits()
    {
        var claim = ApplicationClaims.Draft();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = new AddRiskObjectCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Mapper);
        var command = new AddRiskObjectCommand(claim.Id, ApplicationRequests.Kitchen());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AssetType.Should().Be("Property");
        result.Value.AssetDescription.Should().Be("Kitchen");
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = new AddRiskObjectCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Mapper);
        var command = new AddRiskObjectCommand(claimId, ApplicationRequests.Kitchen());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_ForAValidCommand_HasNoErrors()
    {
        var validator = new AddRiskObjectCommandValidator();
        var command = new AddRiskObjectCommand(Guid.NewGuid(), ApplicationRequests.Kitchen());

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnEmptyClaimId_FailsOnTheField()
    {
        var validator = new AddRiskObjectCommandValidator();
        var command = new AddRiskObjectCommand(Guid.Empty, ApplicationRequests.Kitchen());

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.ClaimId);
    }

    [Fact]
    public void Validate_WithAnUnknownAssetType_FailsOnTheField()
    {
        var validator = new AddRiskObjectCommandValidator();
        var command = new AddRiskObjectCommand(
            Guid.NewGuid(),
            ApplicationRequests.Kitchen() with { AssetType = (AssetType)99 });

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.RiskObject.AssetType);
    }
}
