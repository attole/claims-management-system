using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Parties;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Domain.Claims.Parties;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.Parties;

public sealed class AddPartyCommandTests
{
    [Fact]
    public async Task Handle_WhenTheClaimIsEditable_AddsTheClaimantAndCommits()
    {
        var claim = ApplicationClaims.Draft();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = new AddPartyCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.Mapper);
        var command = new AddPartyCommand(claim.Id, ApplicationRequests.Claimant());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be("Claimant");
        result.Value.FirstName.Should().Be("Ada");
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = new AddPartyCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.Mapper);
        var command = new AddPartyCommand(claimId, ApplicationRequests.Claimant());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_ForAValidCommand_HasNoErrors()
    {
        var validator = new AddPartyCommandValidator();
        var command = new AddPartyCommand(Guid.NewGuid(), ApplicationRequests.Claimant());

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnEmptyClaimId_FailsOnTheField()
    {
        var validator = new AddPartyCommandValidator();
        var command = new AddPartyCommand(Guid.Empty, ApplicationRequests.Claimant());

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.ClaimId);
    }

    [Fact]
    public void Validate_WithAnUnknownRole_FailsOnTheField()
    {
        var validator = new AddPartyCommandValidator();
        var command = new AddPartyCommand(
            Guid.NewGuid(),
            ApplicationRequests.Claimant() with { Role = (PartyRoleKind)99 });

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.Party.Role);
    }
}
