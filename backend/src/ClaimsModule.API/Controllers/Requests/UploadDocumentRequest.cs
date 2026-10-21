using ClaimsModule.Application.ClaimDocuments.Commands;

namespace ClaimsModule.API.Controllers.Requests;

public sealed record UploadDocumentRequest
{
    public required IFormFile File { get; init; }

    public string DocumentType { get; init; } = string.Empty;

    public string Notes { get; init; } = string.Empty;

    public UploadDocumentCommand ToCommand(Guid claimId)
    {
        return new UploadDocumentCommand(
            claimId,
            DocumentType ?? string.Empty,
            File.FileName,
            File.ContentType,
            File.Length)
        {
            Content = File.OpenReadStream(),
            Notes = Notes ?? string.Empty,
        };
    }
}
