using ClaimsModule.Application.Results;

namespace ClaimsModule.Application.ClaimDocuments;

public static class DocumentErrors
{
    public const string NotFoundCode = "DOCUMENT_NOT_FOUND";

    public static Error NotFound(Guid documentId)
    {
        return Error.NotFound(NotFoundCode, $"Document {documentId} does not exist on the claim.");
    }
}
