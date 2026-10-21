using AutoMapper;
using ClaimsModule.Domain.ClaimDocuments;
using ClaimsModule.Domain.ClaimDocuments.Inputs;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Persistence.Readers;
using ClaimsModule.Persistence.Schema.Seed;
using ClaimsModule.Persistence.SqlServerTests.Support;

namespace ClaimsModule.Persistence.SqlServerTests.Readers;

[Collection(SqlServerCollection.Name)]
public sealed class ClaimDocumentReaderTests(SqlServerFixture database)
{
    [SqlServerFact]
    public async Task FindReferencedPathsAsync_WithoutAnOrganisation_ReturnsOnlyPathsADocumentReferences()
    {
        var organisationId = Guid.NewGuid();
        var kept = $"claim-documents/{Guid.NewGuid():N}_kept.pdf";
        var stray = $"claim-documents/{Guid.NewGuid():N}_stray.pdf";
        var claim = SqlServerClaims.Create(organisationId);
        using (var setup = database.CreateContext(organisationId))
        {
            setup.Claims.Add(claim);
            setup.ClaimDocuments.Add(Upload(claim.Id, organisationId, kept));
            await setup.SaveChangesAsync();
        }

        using var jobContext = database.CreateContext(Guid.Empty);
        var reader = new ClaimDocumentReader(jobContext, new Mock<IMapper>().Object);

        var result = await reader.FindReferencedPathsAsync([kept, stray], CancellationToken.None);

        result.Should().Equal(kept);
    }

    private static ClaimDocument Upload(Guid claimId, Guid organisationId, string blobPath)
    {
        var claim = new ClaimReference(claimId, organisationId, new ClaimOpenState());
        var details = new ClaimDocumentDetails("PoliceReport", "kept.pdf", blobPath, "application/pdf", 1_024)
        {
            Notes = "Filed on the day",
        };
        var input = new UploadClaimDocumentInput(claim, details, SeedIds.SystemUser, SqlServerFixture.Now)
        {
            Header = "%PDF-1.7"u8.ToArray(),
        };

        return ClaimDocument.Upload(input).Value;
    }
}
