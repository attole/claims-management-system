using ClaimsModule.Application.Audit.Dtos;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.ClaimDocuments.Dtos;
using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Claims.Queries;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.ReserveComponents.Dtos;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.Queries;

public sealed class GetClaimQueryTests
{
    [Fact]
    [Trait("Rule", "S:API-GET-claim")]
    public async Task Handle_ComposesTheClaimTheReservesTheDocumentsAndTheRecentAudit()
    {
        var claimId = Guid.NewGuid();
        var detail = new ClaimDetailDto { ClientName = "Ada Stone" };
        var reserves = new ClaimReservesDto { ApprovedTotal = 5000m };
        var document = new ClaimDocumentDto { DocumentName = "report.pdf" };
        var entry = new AuditEntryDto { EventType = "CLAIM_CREATED" };
        var context = new QueryContext()
            .WithDetail(claimId, detail)
            .WithReserves(reserves)
            .WithDocuments(new ClaimDocumentFile(document, "claim-documents/a/b/c_report.pdf"))
            .WithAuditPage(new PagedResult<AuditEntryDto>([entry], 1, 10, 1));
        var handler = CreateHandler(context);
        var query = new GetClaimQuery(claimId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.ClientName.Should().Be("Ada Stone");
        result.Value.Reserves.ApprovedTotal.Should().Be(5000m);
        result.Value.Documents.Single().DownloadUrl.Should().Be("https://files/claim-documents/a/b/c_report.pdf");
        result.Value.RecentAudit.Should().Equal(entry);
    }

    [Fact]
    public async Task Handle_AsksForOnlyTheFirstPageOfRecentAudit()
    {
        var claimId = Guid.NewGuid();
        var context = new QueryContext().WithDetail(claimId, new ClaimDetailDto());
        var handler = CreateHandler(context);
        var query = new GetClaimQuery(claimId);

        await handler.Handle(query, CancellationToken.None);

        context.VerifyAuditListed(claimId, new PageWindow(1, 10));
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndReadsNothingElse()
    {
        var claimId = Guid.NewGuid();
        var context = new QueryContext().WithUnknownClaim(claimId);
        var handler = CreateHandler(context);
        var query = new GetClaimQuery(claimId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyNoDocumentRead();
    }

    [Fact]
    public void Validate_WithAnEmptyId_FailsOnTheField()
    {
        var validator = new GetClaimQueryValidator();
        var query = new GetClaimQuery(Guid.Empty);

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(request => request.ClaimId);
    }

    private static GetClaimQueryHandler CreateHandler(QueryContext context)
    {
        return new GetClaimQueryHandler(
            context.Claims,
            context.Reserves,
            context.Documents,
            context.Audit,
            context.Storage);
    }
}
