using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Domain.Policies;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims;

public sealed class LinkPolicyCommandTests
{
    [Fact]
    public async Task Handle_WhenThePolicyExists_LinksItAndCommits()
    {
        var claim = ApplicationClaims.Draft();
        var period = new PolicyPeriod(
            Guid.NewGuid(),
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            DateOnly.MinValue);
        var context = new ClaimHandlerContext().WithClaim(claim).WithPolicy(period);
        var handler = new LinkPolicyCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Mapper);
        var command = new LinkPolicyCommand(claim.Id, period.PolicyId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PolicyId.Should().Be(period.PolicyId);
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = new LinkPolicyCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Mapper);
        var command = new LinkPolicyCommand(claimId, Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenThePolicyDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claim = ApplicationClaims.Draft();
        var unknownId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithClaim(claim).WithoutPolicy(unknownId);
        var handler = new LinkPolicyCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Mapper);
        var command = new LinkPolicyCommand(claim.Id, unknownId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.PolicyNotFound(unknownId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_ForAValidCommand_HasNoErrors()
    {
        var validator = new LinkPolicyCommandValidator();
        var command = new LinkPolicyCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnEmptyClaimId_FailsOnTheField()
    {
        var validator = new LinkPolicyCommandValidator();
        var command = new LinkPolicyCommand(Guid.Empty, Guid.NewGuid());

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.ClaimId);
    }

    [Fact]
    public void Validate_WithAnEmptyPolicyId_FailsOnTheField()
    {
        var validator = new LinkPolicyCommandValidator();
        var command = new LinkPolicyCommand(Guid.NewGuid(), Guid.Empty);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.PolicyId);
    }
}
