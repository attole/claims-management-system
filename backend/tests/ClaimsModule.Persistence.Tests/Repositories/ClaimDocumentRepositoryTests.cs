using ClaimsModule.Application.ClaimDocuments;
using ClaimsModule.Domain.ClaimDocuments;
using ClaimsModule.Domain.Common;
using ClaimsModule.Persistence.Repositories;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.ClaimDocuments;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Repositories;

public sealed class ClaimDocumentRepositoryTests
{
    [Fact]
    public async Task AddAsync_Document_IsTrackedAsAdded()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var repository = new ClaimDocumentRepository(context);
        var document = TestDocuments.Upload(Ids.Next(), TestDatabase.OrganisationId, "a.pdf", database.Now);

        await repository.AddAsync(document, CancellationToken.None);

        context.Entry(document).State.Should().Be(EntityState.Added);
    }

    [Fact]
    public async Task GetAsync_DocumentOfTheClaim_ReturnsIt()
    {
        using var database = new TestDatabase();
        var claimId = Ids.Next();
        var document = await SeedAsync(database, claimId);
        using var context = database.CreateContext();
        var repository = new ClaimDocumentRepository(context);

        var result = await repository.GetAsync(claimId, document.Id, CancellationToken.None);

        result.Value.Details.DocumentName.Should().Be("a.pdf");
    }

    [Fact]
    public async Task GetAsync_DocumentOfAnotherClaim_IsNotFound()
    {
        using var database = new TestDatabase();
        var document = await SeedAsync(database, Ids.Next());
        using var context = database.CreateContext();
        var repository = new ClaimDocumentRepository(context);

        var result = await repository.GetAsync(Ids.Next(), document.Id, CancellationToken.None);

        result.Errors.Should().ContainSingle(error => error.Code == DocumentErrors.NotFoundCode);
    }

    private static async Task<ClaimDocument> SeedAsync(TestDatabase database, Guid claimId)
    {
        using var context = database.CreateContext();
        var document = TestDocuments.Upload(claimId, TestDatabase.OrganisationId, "a.pdf", database.Now);
        context.ClaimDocuments.Add(document);
        await context.SaveChangesAsync();

        return document;
    }
}
