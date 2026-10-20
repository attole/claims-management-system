using ClaimsModule.Domain.Common;
using ClaimsModule.Persistence.Readers;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.ClaimDocuments;
using ClaimsModule.Persistence.Tests.Support.Mapping;

namespace ClaimsModule.Persistence.Tests.Readers;

public sealed class ClaimDocumentReaderTests
{
    [Fact]
    public async Task ListAsync_Claim_ReturnsItsDocumentsOldestFirstWithTheirBlobPaths()
    {
        using var database = new TestDatabase();
        var claimId = Ids.Next();
        using (var writer = database.CreateContext())
        {
            var organisationId = TestDatabase.OrganisationId;
            var later = TestDocuments.Upload(claimId, organisationId, "later.pdf", database.Now.AddDays(1));
            var first = TestDocuments.Upload(claimId, organisationId, "first.pdf", database.Now);
            var other = TestDocuments.Upload(Ids.Next(), organisationId, "other.pdf", database.Now);
            writer.ClaimDocuments.AddRange(later, first, other);
            writer.SaveChanges();
        }

        using var context = database.CreateContext();
        var reader = new ClaimDocumentReader(context, TestMapper.Create());

        var result = await reader.ListAsync(claimId, CancellationToken.None);

        result.Select(file => file.Document.DocumentName).Should().Equal("first.pdf", "later.pdf");
        result[0].BlobPath.Should().Be("claim-documents/first.pdf");
    }

    [Fact]
    public async Task ListAsync_RemovedDocument_IsNotListed()
    {
        using var database = new TestDatabase();
        var claimId = Ids.Next();
        using var context = database.CreateContext();
        var document = TestDocuments.Upload(claimId, TestDatabase.OrganisationId, "a.pdf", database.Now);
        context.ClaimDocuments.Add(document);
        context.SaveChanges();
        context.ClaimDocuments.Remove(document);
        context.SaveChanges();
        var reader = new ClaimDocumentReader(context, TestMapper.Create());

        var result = await reader.ListAsync(claimId, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
