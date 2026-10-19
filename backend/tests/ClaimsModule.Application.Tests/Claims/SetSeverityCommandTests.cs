using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Domain.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims;

public sealed class SetSeverityCommandTests
{
    [Fact]
    public async Task Handle_WhenTheClaimIsEditable_SetsTheSeverityAndCommits()
    {
        var claim = ApplicationClaims.Draft();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = new SetSeverityCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Mapper);
        var command = new SetSeverityCommand(claim.Id, ClaimSeverity.Standard);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Severity.Should().Be("Standard");
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = new SetSeverityCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Mapper);
        var command = new SetSeverityCommand(claimId, ClaimSeverity.Standard);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_ForAValidCommand_HasNoErrors()
    {
        var validator = new SetSeverityCommandValidator();
        var command = new SetSeverityCommand(Guid.NewGuid(), ClaimSeverity.Standard);

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnEmptyClaimId_FailsOnTheField()
    {
        var validator = new SetSeverityCommandValidator();
        var command = new SetSeverityCommand(Guid.Empty, ClaimSeverity.Standard);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.ClaimId);
    }

    [Fact]
    public void Validate_WithAnUnknownSeverity_FailsOnTheField()
    {
        var validator = new SetSeverityCommandValidator();
        var command = new SetSeverityCommand(Guid.NewGuid(), (ClaimSeverity)99);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.Severity);
    }
}
