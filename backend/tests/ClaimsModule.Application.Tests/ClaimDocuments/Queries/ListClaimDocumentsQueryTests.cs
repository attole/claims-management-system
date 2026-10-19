using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.ClaimDocuments.Dtos;
using ClaimsModule.Application.ClaimDocuments.Queries;
using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.ClaimDocuments.Queries;

public sealed class ListClaimDocumentsQueryTests
{
    [Fact]
    public async Task Handle_GivesEachDocumentItsDownloadUrl()
    {
        var claimId = Guid.NewGuid();
        var document = new ClaimDocumentDto { DocumentName = "report.pdf" };
        var context = new QueryContext()
            .WithKnownClaim(claimId)
            .WithDocuments(new ClaimDocumentFile(document, "claim-documents/a/b/c_report.pdf"));
        var handler = new ListClaimDocumentsQueryHandler(context.Claims, context.Documents, context.Storage);
        var query = new ListClaimDocumentsQuery(claimId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.Single().DownloadUrl.Should().Be("https://files/claim-documents/a/b/c_report.pdf");
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFound()
    {
        var claimId = Guid.NewGuid();
        var context = new QueryContext().WithUnknownClaim(claimId);
        var handler = new ListClaimDocumentsQueryHandler(context.Claims, context.Documents, context.Storage);
        var query = new ListClaimDocumentsQuery(claimId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyNoDocumentRead();
    }

    [Fact]
    public void Validate_WithAnEmptyId_FailsOnTheField()
    {
        var validator = new ListClaimDocumentsQueryValidator();
        var query = new ListClaimDocumentsQuery(Guid.Empty);

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(request => request.ClaimId);
    }
}
