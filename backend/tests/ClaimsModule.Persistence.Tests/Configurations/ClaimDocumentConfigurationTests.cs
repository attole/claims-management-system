using ClaimsModule.Domain.Common;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.ClaimDocuments;

namespace ClaimsModule.Persistence.Tests.Configurations;

public sealed class ClaimDocumentConfigurationTests
{
    [Fact]
    public void Configure_Document_ReadsBackItsDetails()
    {
        using var database = new TestDatabase();
        var claimId = Ids.Next();
        using (var writer = database.CreateContext())
        {
            var document = TestDocuments.Upload(claimId, TestDatabase.OrganisationId, "a.pdf", database.Now);
            writer.ClaimDocuments.Add(document);
            writer.SaveChanges();
        }

        using var context = database.CreateContext();

        var result = context.ClaimDocuments.Single();

        result.ClaimId.Should().Be(claimId);
        result.Details.DocumentName.Should().Be("a.pdf");
        result.Details.FileSizeBytes.Should().Be(1_024);
        result.Details.Notes.Should().Be("Filed on the day");
        result.UploadedAt.Should().Be(database.Now);
    }

    [Fact]
    public void Configure_RemovedDocument_IsHiddenFromQueries()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var document = TestDocuments.Upload(Ids.Next(), TestDatabase.OrganisationId, "a.pdf", database.Now);
        context.ClaimDocuments.Add(document);
        context.SaveChanges();
        context.ClaimDocuments.Remove(document);
        context.SaveChanges();

        var result = context.ClaimDocuments.ToList();

        result.Should().BeEmpty();
    }
}
