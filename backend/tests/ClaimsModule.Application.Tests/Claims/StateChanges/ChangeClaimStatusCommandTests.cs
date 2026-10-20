using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.StateChanges;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Core.Flows;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.StateChanges;

public sealed class ChangeClaimStatusCommandTests
{
    [Fact]
    [Trait("Rule", "S:ST-Draft-Open")]
    public async Task Handle_OpeningACompleteDraft_ReturnsTheMoveAndCommits()
    {
        var claim = ApplicationClaims.ReadyDraft();
        var context = CreateContext(claim);
        var handler = CreateHandler(context);
        var command = new ChangeClaimStatusCommand(claim.Id, ClaimStateKind.Open);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().Be(new ClaimStatusDto(claim.Id, "Draft", "Open"));
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    [Trait("Rule", "S:ST-Open-UnderInvestigation")]
    public async Task Handle_InvestigatingAnOpenClaim_ReturnsTheMove()
    {
        var claim = ApplicationClaims.Opened();
        var context = CreateContext(claim);
        var handler = CreateHandler(context);
        var command = new ChangeClaimStatusCommand(claim.Id, ClaimStateKind.UnderInvestigation);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().Be(new ClaimStatusDto(claim.Id, "Open", "UnderInvestigation"));
    }

    [Fact]
    [Trait("Rule", "S:ST-Open-PendingPayment")]
    public async Task Handle_SendingToPaymentWithAnApprovedReserve_ReturnsTheMove()
    {
        var claim = ApplicationClaims.Opened();
        var context = CreateContext(claim).WithReserveFacts(new ClaimReserveFacts(false, false, true, true, 5000m));
        var handler = CreateHandler(context);
        var command = new ChangeClaimStatusCommand(claim.Id, ClaimStateKind.PendingPayment);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().Be(new ClaimStatusDto(claim.Id, "Open", "PendingPayment"));
    }

    [Fact]
    [Trait("Rule", "S:ST-Open-PendingPayment")]
    public async Task Handle_SendingToPaymentWithoutAnApprovedReserve_ReturnsTheRuleErrorAndDoesNotCommit()
    {
        var claim = ApplicationClaims.Opened();
        var context = CreateContext(claim);
        var handler = CreateHandler(context);
        var command = new ChangeClaimStatusCommand(claim.Id, ClaimStateKind.PendingPayment);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Select(error => error.Code).Should().Equal("S:ST-Open-PendingPayment");
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    [Trait("Rule", "S:ST-Open-Closed")]
    public async Task Handle_ClosingAnOpenClaim_ReturnsTheMove()
    {
        var claim = ApplicationClaims.Opened();
        var context = CreateContext(claim);
        var handler = CreateHandler(context);
        var command = new ChangeClaimStatusCommand(claim.Id, ClaimStateKind.Closed);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().Be(new ClaimStatusDto(claim.Id, "Open", "Closed"));
    }

    [Fact]
    [Trait("Rule", "S:ST-Open-Withdrawn")]
    public async Task Handle_WithdrawingWithAReason_ReturnsTheMove()
    {
        var claim = ApplicationClaims.Opened();
        var context = CreateContext(claim);
        var handler = CreateHandler(context);
        var command = new ChangeClaimStatusCommand(claim.Id, ClaimStateKind.Withdrawn)
        {
            ReasonCode = "WITHDRAWN",
            ReasonText = "Reported in error",
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().Be(new ClaimStatusDto(claim.Id, "Open", "Withdrawn"));
    }

    [Fact]
    [Trait("Rule", "S:ST-Closed-Reopened")]
    public async Task Handle_ReopeningAClosedClaim_ReportsTheStateItEndsIn()
    {
        var claim = ApplicationClaims.Closed();
        var context = CreateContext(claim);
        var handler = CreateHandler(context);
        var command = new ChangeClaimStatusCommand(claim.Id, ClaimStateKind.Reopened)
        {
            ReasonCode = "REOPEN",
            ReasonText = "New evidence",
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Should().Be(new ClaimStatusDto(claim.Id, "Closed", "Open"));
    }

    [Fact]
    [Trait("Rule", ClaimErrors.StatusUnsupportedCode)]
    public async Task Handle_ForATargetNoMoveLeadsTo_ReturnsUnsupportedAndDoesNotCommit()
    {
        var claim = ApplicationClaims.ReadyDraft();
        var context = CreateContext(claim);
        var handler = CreateHandler(context);
        var command = new ChangeClaimStatusCommand(claim.Id, ClaimStateKind.Draft);

        var result = await handler.Handle(command, CancellationToken.None);

        var error = result.Errors.Should().ContainSingle().Subject;
        error.Code.Should().Be(ClaimErrors.StatusUnsupportedCode);
        error.Message.Should().Contain("Valid next statuses: Open");
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    [Trait("Rule", "S:BR-ST-01")]
    public async Task Handle_ForAMoveTheStateMachineDoesNotAllow_ListsTheValidNextStatuses()
    {
        var claim = ApplicationClaims.ReadyDraft();
        var context = CreateContext(claim);
        var handler = CreateHandler(context);
        var command = new ChangeClaimStatusCommand(claim.Id, ClaimStateKind.Closed);

        var result = await handler.Handle(command, CancellationToken.None);

        var error = result.Errors.Should().ContainSingle().Subject;
        error.Code.Should().Be(TransitionCodes.NotPermitted);
        error.Message.Should().Contain("Valid next statuses: Open");
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = CreateHandler(context);
        var command = new ChangeClaimStatusCommand(claimId, ClaimStateKind.Open);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_ForAValidCommand_HasNoErrors()
    {
        var validator = new ChangeClaimStatusCommandValidator();
        var command = new ChangeClaimStatusCommand(Guid.NewGuid(), ClaimStateKind.Open);

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnEmptyClaimId_FailsOnTheField()
    {
        var validator = new ChangeClaimStatusCommandValidator();
        var command = new ChangeClaimStatusCommand(Guid.Empty, ClaimStateKind.Open);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.ClaimId);
    }

    [Fact]
    public void Validate_WithAReasonCodeLongerThanTheColumn_FailsOnTheField()
    {
        var validator = new ChangeClaimStatusCommandValidator();
        var command = new ChangeClaimStatusCommand(Guid.NewGuid(), ClaimStateKind.Withdrawn)
        {
            ReasonCode = new string('x', ClaimFieldLengths.ReasonCode + 1),
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.ReasonCode);
    }

    [Fact]
    public void Validate_WithAReasonTextLongerThanTheColumn_FailsOnTheField()
    {
        var validator = new ChangeClaimStatusCommandValidator();
        var command = new ChangeClaimStatusCommand(Guid.NewGuid(), ClaimStateKind.Withdrawn)
        {
            ReasonText = new string('x', ClaimFieldLengths.ReasonText + 1),
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.ReasonText);
    }

    [Fact]
    public void Validate_WithAnUnknownStatus_FailsOnTheField()
    {
        var validator = new ChangeClaimStatusCommandValidator();
        var command = new ChangeClaimStatusCommand(Guid.NewGuid(), (ClaimStateKind)99);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.TargetStatus);
    }

    private static ClaimHandlerContext CreateContext(Claim claim)
    {
        return new ClaimHandlerContext()
            .WithClaim(claim)
            .WithPolicy(ApplicationClaims.LinkedPeriod());
    }

    private static ChangeClaimStatusCommandHandler CreateHandler(ClaimHandlerContext context)
    {
        return new ChangeClaimStatusCommandHandler(
            context.Claims,
            context.Reserves,
            context.Policies,
            context.Causes,
            context.UnitOfWork,
            context.Clock,
            context.User);
    }
}
