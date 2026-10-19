using ClaimsModule.Application.ClaimDocuments;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.ClaimDocuments.Dtos;
using ClaimsModule.Application.Tests.Support.Claims;

namespace ClaimsModule.Application.Tests.ClaimDocuments;

public sealed class ClaimDocumentLinksTests
{
    [Fact]
    public async Task BuildAsync_PairsEachDocumentWithItsUrlInOrder()
    {
        var context = new QueryContext();
        var first = new ClaimDocumentFile(new ClaimDocumentDto { DocumentName = "a.pdf" }, "path/a");
        var second = new ClaimDocumentFile(new ClaimDocumentDto { DocumentName = "b.pdf" }, "path/b");

        var result = await ClaimDocumentLinks.BuildAsync(context.Storage, [first, second], CancellationToken.None);

        result.Select(link => link.DownloadUrl).Should().Equal("https://files/path/a", "https://files/path/b");
        result.Select(link => link.Document.DocumentName).Should().Equal("a.pdf", "b.pdf");
    }

    [Fact]
    public async Task BuildAsync_ForNoDocuments_ReturnsNoLinks()
    {
        var context = new QueryContext();

        var result = await ClaimDocumentLinks.BuildAsync(context.Storage, [], CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:BR-D-02")]
    public async Task BuildAsync_AsksForAOneHourDownloadUrl()
    {
        var context = new QueryContext();
        var file = new ClaimDocumentFile(new ClaimDocumentDto { DocumentName = "a.pdf" }, "path/a");

        await ClaimDocumentLinks.BuildAsync(context.Storage, [file], CancellationToken.None);

        context.VerifyDownloadUrlRequested("path/a", TimeSpan.FromHours(1));
    }
}
