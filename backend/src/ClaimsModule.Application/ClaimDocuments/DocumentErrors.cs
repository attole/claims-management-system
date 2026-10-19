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

public static class DocumentFieldLengths
{
    public const int DocumentType = 100;
    public const int DocumentName = 255;
    public const int ContentType = 100;
    public const int BlobPath = 500;
    public const int Notes = 500;
}
