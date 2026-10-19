using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Commands;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.Commands;

public sealed class SetLimitOverrideCommandTests
{
    [Fact]
    public async Task Handle_ByAManager_SetsTheOverrideAndCommits()
    {
        var claim = ApplicationClaims.ReadyDraft();
        var context = new ClaimHandlerContext()
            .WithClaim(claim)
            .WithPolicy(ApplicationClaims.LinkedPeriod());
        var handler = CreateHandler(context);
        var command = new SetLimitOverrideCommand(claim.Id, true);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.HasAggregateLimitOverride.Should().BeTrue();
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = CreateHandler(context);
        var command = new SetLimitOverrideCommand(claimId, true);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_ForAValidCommand_HasNoErrors()
    {
        var validator = new SetLimitOverrideCommandValidator();
        var command = new SetLimitOverrideCommand(Guid.NewGuid(), true);

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnEmptyClaimId_FailsOnTheField()
    {
        var validator = new SetLimitOverrideCommandValidator();
        var command = new SetLimitOverrideCommand(Guid.Empty, true);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.ClaimId);
    }

    private static SetLimitOverrideCommandHandler CreateHandler(ClaimHandlerContext context)
    {
        return new SetLimitOverrideCommandHandler(
            context.Claims,
            context.Reserves,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.User,
            context.Mapper);
    }
}
