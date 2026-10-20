using ClaimsModule.Application.ClaimDocuments;
using ClaimsModule.Application.ClaimDocuments.Commands;
using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Tests.Support.ClaimDocuments;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Domain.ClaimDocuments.Inputs;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.ClaimDocuments.Commands;

public sealed class RemoveDocumentCommandTests
{
    [Fact]
    public async Task Handle_ForAnExistingDocument_SoftDeletesItAndCommits()
    {
        var claim = ApplicationClaims.Opened();
        var document = ApplicationDocuments.Uploaded(claim);
        var context = new ClaimHandlerContext().WithClaim(claim).WithDocument(document);
        var handler = CreateHandler(context);
        var command = new RemoveDocumentCommand(claim.Id, document.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.IsDeleted.Should().BeTrue();
        context.VerifyCommitted(Times.Once());
    }

    [Fact]
    [Trait("Rule", "DOCUMENT_REMOVED")]
    public async Task Handle_ForAnAlreadyRemovedDocument_ReturnsTheRuleErrorAndDoesNotCommit()
    {
        var claim = ApplicationClaims.Opened();
        var document = ApplicationDocuments.Uploaded(claim);
        document.Remove(new RemoveClaimDocumentInput(claim.State));
        var context = new ClaimHandlerContext().WithClaim(claim).WithDocument(document);
        var handler = CreateHandler(context);
        var command = new RemoveDocumentCommand(claim.Id, document.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Select(error => error.Code).Should().Equal("DOCUMENT_REMOVED");
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public async Task Handle_ForAnUnknownDocument_ReturnsNotFoundAndDoesNotCommit()
    {
        var claim = ApplicationClaims.Opened();
        var documentId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithClaim(claim).WithoutDocument(documentId);
        var handler = CreateHandler(context);
        var command = new RemoveDocumentCommand(claim.Id, documentId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(DocumentErrors.NotFound(documentId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenTheClaimDoesNotExist_ReturnsNotFoundAndDoesNotCommit()
    {
        var claimId = Guid.NewGuid();
        var context = new ClaimHandlerContext().WithoutClaim(claimId);
        var handler = CreateHandler(context);
        var command = new RemoveDocumentCommand(claimId, Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Errors.Should().Equal(ClaimErrors.ClaimNotFound(claimId));
        context.VerifyCommitted(Times.Never());
    }

    [Fact]
    public void Validate_WithAnEmptyId_FailsOnTheField()
    {
        var validator = new RemoveDocumentCommandValidator();
        var command = new RemoveDocumentCommand(Guid.NewGuid(), Guid.Empty);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(request => request.DocumentId);
    }

    [Fact]
    public void Validate_ForValidIds_HasNoErrors()
    {
        var validator = new RemoveDocumentCommandValidator();
        var command = new RemoveDocumentCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static RemoveDocumentCommandHandler CreateHandler(ClaimHandlerContext context)
    {
        return new RemoveDocumentCommandHandler(
            context.Claims,
            context.Documents,
            context.UnitOfWork,
            context.Mapper);
    }
}
