using ClaimsModule.Domain.ClaimDocuments;
using ClaimsModule.Domain.ClaimDocuments.Inputs;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Support.ClaimDocuments;

public static class TestDocuments
{
    public const string Pdf = "application/pdf";

    public static readonly byte[] PdfHeader = "%PDF-1.7"u8.ToArray();

    public static readonly Guid ClaimId = Guid.Parse("88888888-8888-8888-8888-888888888888");

    public static ClaimReference Claim(ClaimState state)
    {
        return new ClaimReference(ClaimId, TestClaims.Organisation, state);
    }

    public static ClaimDocumentDetails Details()
    {
        return new ClaimDocumentDetails(
            "PoliceReport",
            "report.pdf",
            "claim-documents/org/claim/report.pdf",
            Pdf,
            1_024)
        {
            Notes = "Filed on the day",
        };
    }

    public static UploadClaimDocumentInput UploadInput()
    {
        return new UploadClaimDocumentInput(Claim(new ClaimOpenState()), Details(), TestClaims.Actor, TestClaims.Start)
        {
            Header = PdfHeader,
        };
    }

    public static ClaimDocument Upload()
    {
        var document = ClaimDocument.Upload(UploadInput()).Value;
        document.PopEvents();

        return document;
    }
}
