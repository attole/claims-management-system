using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Parties;
using ClaimsModule.Application.Results;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.Parties;

public sealed class RemovePartyCommandTests
{
    [Fact]
    public async Task Handle_WhenAnotherClaimantRemains_RemovesThePartyAndCommits()
    {
        var claim = ApplicationClaims.WithParties();
        var witness = claim.Parties.Single(party => !party.IsActiveClaimant);
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = new RemovePartyCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.Mapper);
        var command = new RemovePartyCommand(claim.Id, witness.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsActive.Should().BeFalse();
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = new RemovePartyCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.Mapper);
        var command = new RemovePartyCommand(claimId, Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenThePartyDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claim = ApplicationClaims.WithParties();
        var unknownId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = new RemovePartyCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.Mapper);
        var command = new RemovePartyCommand(claim.Id, unknownId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.PartyNotFound(unknownId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    [Trait("Rule", "S:PARTY-LastClaimant")]
    public async Task Handle_WhenItIsTheLastClaimant_ReturnsTheRuleErrorAndDoesNotCommit()
    {
        var claim = ApplicationClaims.WithParties();
        var claimant = claim.Parties.Single(party => party.IsActiveClaimant);
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = new RemovePartyCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.Mapper);
        var command = new RemovePartyCommand(claim.Id, claimant.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Select(error => error.Code).Should().Equal("S:PARTY-LastClaimant");
        result.Errors.Should().OnlyContain(error => error.Kind == ErrorKind.Validation);
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_ForAValidCommand_HasNoErrors()
    {
        var validator = new RemovePartyCommandValidator();
        var command = new RemovePartyCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnEmptyClaimId_FailsOnTheField()
    {
        var validator = new RemovePartyCommandValidator();
        var command = new RemovePartyCommand(Guid.Empty, Guid.NewGuid());

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.ClaimId);
    }

    [Fact]
    public void Validate_WithAnEmptyPartyId_FailsOnTheField()
    {
        var validator = new RemovePartyCommandValidator();
        var command = new RemovePartyCommand(Guid.NewGuid(), Guid.Empty);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.PartyId);
    }
}
