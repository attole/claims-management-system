using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Queries;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.Queries;

public sealed class CheckClaimStatusQueryTests
{
    [Fact]
    [Trait("Rule", "S:ST-Open-PendingPayment")]
    public async Task Handle_TargetTheClaimCannotReach_ReturnsTheBlockingErrorsAndDoesNotCommit()
    {
        var claim = ApplicationClaims.Opened();
        var context = CreateContext(claim);
        var handler = CreateHandler(context);
        var query = new CheckClaimStatusQuery(claim.Id, ClaimStateKind.PendingPayment);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.Errors.Select(error => error.Code).Should().Equal("S:ST-Open-PendingPayment");
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    [Trait("Rule", "S:ST-Open-UnderInvestigation")]
    public async Task Handle_TargetTheClaimCanReach_ReturnsNoErrorsAndDoesNotCommit()
    {
        var claim = ApplicationClaims.Opened();
        var context = CreateContext(claim);
        var handler = CreateHandler(context);
        var query = new CheckClaimStatusQuery(claim.Id, ClaimStateKind.UnderInvestigation);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.Errors.Should().BeEmpty();
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    [Trait("Rule", "S:CC-01")]
    public async Task Handle_ClosingWithAPendingReserve_ReportsTheBlockingCondition()
    {
        var claim = ApplicationClaims.Opened();
        var context = CreateContext(claim).WithReserveFacts(new ClaimReserveFacts(true, false, false, false, 0m));
        var handler = CreateHandler(context);
        var query = new CheckClaimStatusQuery(claim.Id, ClaimStateKind.Closed);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.Errors.Select(error => error.Code).Should().Contain("S:CC-01");
    }

    [Fact]
    [Trait("Rule", "S:CC-02")]
    public async Task Handle_ClosingWithAnUnresolvedCriticalIssue_ReportsTheBlockingCondition()
    {
        var claim = ApplicationClaims.Opened();
        var context = CreateContext(claim).WithActiveCauseCodes(new CauseCodes(["OTHER"]));
        var handler = CreateHandler(context);
        var query = new CheckClaimStatusQuery(claim.Id, ClaimStateKind.Closed);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.Errors.Select(error => error.Code).Should().Contain("S:CC-02");
    }

    [Fact]
    [Trait("Rule", "S:CC-04")]
    public async Task Handle_ClosingWithAnOpenReserveBalance_ReportsTheJustificationCondition()
    {
        var claim = ApplicationClaims.Opened();
        var context = CreateContext(claim).WithReserveFacts(new ClaimReserveFacts(false, false, true, true, 5000m));
        var handler = CreateHandler(context);
        var query = new CheckClaimStatusQuery(claim.Id, ClaimStateKind.Closed);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.Errors.Select(error => error.Code).Should().Contain("S:CC-04");
    }

    [Fact]
    public async Task Handle_UnknownClaim_ReturnsTheNotFoundError()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = CreateHandler(context);
        var query = new CheckClaimStatusQuery(claimId, ClaimStateKind.Open);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
    }

    [Fact]
    public void Validate_EmptyClaimId_HasAnError()
    {
        var validator = new CheckClaimStatusQueryValidator();
        var query = new CheckClaimStatusQuery(Guid.Empty, ClaimStateKind.Open);

        var outcome = validator.TestValidate(query);

        outcome.ShouldHaveValidationErrorFor(item => item.ClaimId);
    }

    private static ClaimHandlerContext CreateContext(Claim claim)
    {
        return new ClaimHandlerContext()
            .WithClaim(claim)
            .WithPolicy(ApplicationClaims.LinkedPeriod());
    }

    private static CheckClaimStatusQueryHandler CreateHandler(ClaimHandlerContext context)
    {
        return new CheckClaimStatusQueryHandler(
            context.Claims,
            context.Reserves,
            context.Policies,
            context.Causes,
            context.Clock,
            context.User);
    }
}
