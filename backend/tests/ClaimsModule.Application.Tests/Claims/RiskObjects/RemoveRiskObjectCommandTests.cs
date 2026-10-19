using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.RiskObjects;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.RiskObjects;

public sealed class RemoveRiskObjectCommandTests
{
    [Fact]
    public async Task Handle_WhenTheRiskObjectExists_RemovesItAndCommits()
    {
        var claim = ApplicationClaims.WithRiskObjects();
        var riskObject = claim.RiskObjects[1];
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = new RemoveRiskObjectCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.Mapper);
        var command = new RemoveRiskObjectCommand(claim.Id, riskObject.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsDeleted.Should().BeTrue();
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = new RemoveRiskObjectCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.Mapper);
        var command = new RemoveRiskObjectCommand(claimId, Guid.NewGuid());

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
        var handler = new RemoveRiskObjectCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.Mapper);
        var command = new RemoveRiskObjectCommand(claim.Id, unknownId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.RiskObjectNotFound(unknownId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_ForAValidCommand_HasNoErrors()
    {
        var validator = new RemoveRiskObjectCommandValidator();
        var command = new RemoveRiskObjectCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnEmptyClaimId_FailsOnTheField()
    {
        var validator = new RemoveRiskObjectCommandValidator();
        var command = new RemoveRiskObjectCommand(Guid.Empty, Guid.NewGuid());

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.ClaimId);
    }

    [Fact]
    public void Validate_WithAnEmptyRiskObjectId_FailsOnTheField()
    {
        var validator = new RemoveRiskObjectCommandValidator();
        var command = new RemoveRiskObjectCommand(Guid.NewGuid(), Guid.Empty);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.RiskObjectId);
    }
}
