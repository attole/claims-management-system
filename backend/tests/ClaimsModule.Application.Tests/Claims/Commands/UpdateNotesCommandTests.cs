using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Commands;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.Commands;

public sealed class UpdateNotesCommandTests
{
    [Fact]
    public async Task Handle_WhenTheClaimIsEditable_UpdatesTheNotesAndCommits()
    {
        var claim = ApplicationClaims.Draft();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = new UpdateNotesCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Mapper);
        var command = new UpdateNotesCommand(claim.Id, "Called the insured");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Notes.Should().Be("Called the insured");
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = new UpdateNotesCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Mapper);
        var command = new UpdateNotesCommand(claimId, "Called the insured");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_ForAValidCommand_HasNoErrors()
    {
        var validator = new UpdateNotesCommandValidator();
        var command = new UpdateNotesCommand(Guid.NewGuid(), "Called the insured");

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnEmptyClaimId_FailsOnTheField()
    {
        var validator = new UpdateNotesCommandValidator();
        var command = new UpdateNotesCommand(Guid.Empty, "Called the insured");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.ClaimId);
    }

    [Fact]
    public void Validate_WithNullNotes_FailsOnTheField()
    {
        var validator = new UpdateNotesCommandValidator();
        var command = new UpdateNotesCommand(Guid.NewGuid(), null!);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.Notes);
    }
}
