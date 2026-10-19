using ClaimsModule.Application.ClaimDocuments;
using ClaimsModule.Application.ClaimDocuments.Commands;
using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Tests.Support.Claims;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.ClaimDocuments.Commands;

public sealed class UploadDocumentCommandTests
{
    [Fact]
    [Trait("Rule", "S:API-POST-claims-documents")]
    public async Task Handle_ForAnAllowedFile_StoresTheBlobAndTheRecord()
    {
        var claim = ApplicationClaims.Opened();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = CreateHandler(context);
        var command = CreateCommand(claim.Id, "report.pdf", "application/pdf");
        var folder = $"claim-documents/{claim.OrganisationId}/{claim.Id}/";

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.DocumentName.Should().Be("report.pdf");
        context.VerifyUploaded(path => path.StartsWith(folder) && path.EndsWith("_report.pdf"), Times.Once());
        context.AddedDocuments.Should().ContainSingle();
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    public async Task Handle_ForAnUnsafeFileName_StoresItUnderTheSanitisedPathAndKeepsTheOriginalName()
    {
        var claim = ApplicationClaims.Opened();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = CreateHandler(context);
        var command = CreateCommand(claim.Id, "..\\my report (1).pdf", "application/pdf");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.DocumentName.Should().Be("..\\my report (1).pdf");
        context.VerifyUploaded(path => path.EndsWith("_my_report__1_.pdf"), Times.Once());
    }

    [Fact]
    [Trait("Rule", "S:DOC-MimeAllowlist")]
    public async Task Handle_ForADisallowedContentType_ReturnsTheRuleErrorAndStoresNothing()
    {
        var claim = ApplicationClaims.Opened();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = CreateHandler(context);
        var command = CreateCommand(claim.Id, "run.exe", "application/x-msdownload");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Select(error => error.Code).Should().Equal("S:DOC-MimeAllowlist");
        context.VerifyAnyUpload(Times.Never());
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    [Trait("Rule", "S:DOC-MaxSize")]
    public async Task Handle_ForATooLargeFile_ReturnsTheRuleErrorAndStoresNothing()
    {
        var claim = ApplicationClaims.Opened();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = CreateHandler(context);
        var command = CreateCommand(claim.Id, "scan.pdf", "application/pdf") with
        {
            FileSizeBytes = 50L * 1024 * 1024 + 1,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Select(error => error.Code).Should().Equal("S:DOC-MaxSize");
        context.VerifyAnyUpload(Times.Never());
    }

    [Fact]
    [Trait("Rule", "DOCUMENT_CLAIM_NOT_EDITABLE")]
    public async Task Handle_ForAFrozenClaim_ReturnsTheRuleErrorAndStoresNothing()
    {
        var claim = ApplicationClaims.Closed();
        var context = new ClaimHandlerContext().WithClaim(claim);
        var handler = CreateHandler(context);
        var command = CreateCommand(claim.Id, "report.pdf", "application/pdf");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Select(error => error.Code).Should().Equal("DOCUMENT_CLAIM_NOT_EDITABLE");
        context.VerifyAnyUpload(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndStoresNothing()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = CreateHandler(context);
        var command = CreateCommand(claimId, "report.pdf", "application/pdf");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyAnyUpload(Times.Never());
    }

    [Fact]
    public void Validate_ForAValidCommand_HasNoErrors()
    {
        var validator = new UploadDocumentCommandValidator();
        var command = CreateCommand(Guid.NewGuid(), "report.pdf", "application/pdf");

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithoutAFileName_FailsOnTheField()
    {
        var validator = new UploadDocumentCommandValidator();
        var command = CreateCommand(Guid.NewGuid(), string.Empty, "application/pdf");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.FileName);
    }

    [Fact]
    public void Validate_WithATooLongFileName_FailsOnTheField()
    {
        var validator = new UploadDocumentCommandValidator();
        var longName = new string('x', DocumentFieldLengths.DocumentName + 1);
        var command = CreateCommand(Guid.NewGuid(), longName, "text/plain");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.FileName);
    }

    [Fact]
    public void Validate_WithAnEmptyClaimId_FailsOnTheField()
    {
        var validator = new UploadDocumentCommandValidator();
        var command = CreateCommand(Guid.Empty, "report.pdf", "application/pdf");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.ClaimId);
    }

    [Fact]
    public void Validate_WithANegativeSize_FailsOnTheField()
    {
        var validator = new UploadDocumentCommandValidator();
        var command = CreateCommand(Guid.NewGuid(), "report.pdf", "application/pdf") with { FileSizeBytes = -1 };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.FileSizeBytes);
    }

    private static UploadDocumentCommandHandler CreateHandler(ClaimHandlerContext context)
    {
        return new UploadDocumentCommandHandler(
            context.Claims,
            context.Documents,
            context.Storage,
            context.UnitOfWork,
            context.Clock,
            context.User,
            context.Mapper);
    }

    private static UploadDocumentCommand CreateCommand(Guid claimId, string fileName, string contentType)
    {
        return new UploadDocumentCommand(claimId, "PoliceReport", fileName, contentType, 2048)
        {
            Content = new MemoryStream([1, 2, 3]),
        };
    }
}
