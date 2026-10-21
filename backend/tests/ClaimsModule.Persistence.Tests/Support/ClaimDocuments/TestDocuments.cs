using ClaimsModule.Domain.ClaimDocuments;
using ClaimsModule.Domain.ClaimDocuments.Inputs;
using ClaimsModule.Domain.Claims;

namespace ClaimsModule.Persistence.Tests.Support.ClaimDocuments;

internal static class TestDocuments
{
    public static ClaimDocument Upload(Guid claimId, Guid organisationId, string name, DateTimeOffset now)
    {
        var claim = new ClaimReference(claimId, organisationId, new ClaimOpenState());
        var blobPath = $"claim-documents/{name}";
        var details = new ClaimDocumentDetails("PoliceReport", name, blobPath, "application/pdf", 1_024)
        {
            Notes = "Filed on the day",
        };
        var input = new UploadClaimDocumentInput(claim, details, TestDatabase.UserId, now)
        {
            Header = "%PDF-1.7"u8.ToArray(),
        };

        return ClaimDocument.Upload(input).Value;
    }
}
