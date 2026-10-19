using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Issues;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.Issues;

public sealed class AcknowledgeIssueCommandTests
{
    [Fact]
    public async Task Handle_WhenTheIssueIsAWarning_AcknowledgesItAndCommits()
    {
        var claim = ApplicationClaims.Draft();
        var issue = claim.Issues.First(issue => issue.IsUnresolvedWarning);
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = new AcknowledgeIssueCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.User,
            context.Mapper);
        var command = new AcknowledgeIssueCommand(claim.Id, issue.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be("Acknowledged");
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = new AcknowledgeIssueCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.User,
            context.Mapper);
        var command = new AcknowledgeIssueCommand(claimId, Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenTheIssueDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claim = ApplicationClaims.Draft();
        var unknownId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = new AcknowledgeIssueCommandHandler(
            context.Claims,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.User,
            context.Mapper);
        var command = new AcknowledgeIssueCommand(claim.Id, unknownId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.IssueNotFound(unknownId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_ForAValidCommand_HasNoErrors()
    {
        var validator = new AcknowledgeIssueCommandValidator();
        var command = new AcknowledgeIssueCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnEmptyClaimId_FailsOnTheField()
    {
        var validator = new AcknowledgeIssueCommandValidator();
        var command = new AcknowledgeIssueCommand(Guid.Empty, Guid.NewGuid());

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.ClaimId);
    }

    [Fact]
    public void Validate_WithAnEmptyIssueId_FailsOnTheField()
    {
        var validator = new AcknowledgeIssueCommandValidator();
        var command = new AcknowledgeIssueCommand(Guid.NewGuid(), Guid.Empty);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(command => command.IssueId);
    }
}
