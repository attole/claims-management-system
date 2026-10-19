using ClaimsModule.Application.Audit;
using ClaimsModule.Application.Audit.Queries;
using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Audit.Queries;

public sealed class ListClaimAuditQueryTests
{
    [Fact]
    [Trait("Rule", "S:API-GET-claims-audit")]
    public async Task Handle_ReturnsTheRequestedPageOfAKnownClaim()
    {
        var claimId = Guid.NewGuid();
        var page = new PagedResult<AuditEntryDto>([new AuditEntryDto { EventType = "PARTY_ADDED" }], 3, 25, 80);
        var context = new QueryContext().WithKnownClaim(claimId).WithAuditPage(page);
        var handler = new ListClaimAuditQueryHandler(context.Claims, context.Audit);
        var query = new ListClaimAuditQuery(claimId, 3, 25);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.Should().Be(page);
        context.VerifyAuditListed(claimId, new PageWindow(3, 25));
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFound()
    {
        var claimId = Guid.NewGuid();
        var context = new QueryContext().WithUnknownClaim(claimId);
        var handler = new ListClaimAuditQueryHandler(context.Claims, context.Audit);
        var query = new ListClaimAuditQuery(claimId, 1, 25);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
    }

    [Fact]
    public void Validate_WithAnEmptyId_FailsOnTheField()
    {
        var validator = new ListClaimAuditQueryValidator();
        var query = new ListClaimAuditQuery(Guid.Empty, 1, 25);

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(request => request.ClaimId);
    }

    [Fact]
    public void Validate_WithAPageSizeOverTheLimit_FailsOnThePageSize()
    {
        var validator = new ListClaimAuditQueryValidator();
        var query = new ListClaimAuditQuery(Guid.NewGuid(), 1, PageLimits.MaxPageSize + 1);

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(request => request.PageSize);
    }
}
