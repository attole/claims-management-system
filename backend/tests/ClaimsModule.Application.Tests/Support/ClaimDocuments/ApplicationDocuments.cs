using ClaimsModule.Domain.ClaimDocuments;
using ClaimsModule.Domain.ClaimDocuments.Inputs;
using ClaimsModule.Domain.Claims;

namespace ClaimsModule.Application.Tests.Support.ClaimDocuments;

public static class ApplicationDocuments
{
    public static ClaimDocument Uploaded(Claim claim)
    {
        var details = new ClaimDocumentDetails(
            "PoliceReport",
            "report.pdf",
            "claim-documents/path/report.pdf",
            "application/pdf",
            2048);
        var uploadedAt = new DateTimeOffset(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);
        var input = new UploadClaimDocumentInput(claim.Reference, details, Guid.NewGuid(), uploadedAt)
        {
            Header = "%PDF-1.7"u8.ToArray(),
        };

        return ClaimDocument.Upload(input).Value;
    }
}
