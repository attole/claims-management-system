using ClaimsModule.Infrastructure.Storage;
using ClaimsModule.Infrastructure.Storage.Local;
using ClaimsModule.Infrastructure.Tests.Support;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.Tests.Storage.Local;

public sealed class LocalDocumentDownloadsTests : IDisposable
{
    private static readonly TimeSpan _oneHour = TimeSpan.FromHours(1);
    private static readonly byte[] _content = [1, 2, 3, 4];

    private readonly string _root = StorageTestData.NewRoot();

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    [Trait("Rule", "S:BR-D-03")]
    public async Task Open_ValidTokenOfAStoredFile_ReturnsTheFileWithItsNameAndType()
    {
        var (downloads, tokens, storage) = Create(StorageTestData.Local(_root));
        await storage.UploadAsync(StorageTestData.BlobPath, new MemoryStream(_content), "application/pdf", default);
        string token = tokens.Create(StorageTestData.BlobPath, _oneHour);

        var result = downloads.Open(token);

        using var content = result.Content;
        result.Should().BeEquivalentTo(new { IsFound = true, FileName = "file.pdf", ContentType = "application/pdf" });
    }

    [Fact]
    [Trait("Rule", "S:BR-D-03")]
    public async Task Open_FileWithAnUnknownExtension_ReturnsOctetStream()
    {
        const string path = "claim-documents/org-1/claim-1/upload_notes.zzz";
        var (downloads, tokens, storage) = Create(StorageTestData.Local(_root));
        await storage.UploadAsync(path, new MemoryStream(_content), "text/plain", default);
        string token = tokens.Create(path, _oneHour);

        var result = downloads.Open(token);

        using var content = result.Content;
        result.ContentType.Should().Be("application/octet-stream");
    }

    [Fact]
    [Trait("Rule", "S:BR-D-03")]
    public async Task Open_TamperedToken_ReturnsNothing()
    {
        var (downloads, tokens, storage) = Create(StorageTestData.Local(_root));
        await storage.UploadAsync(StorageTestData.BlobPath, new MemoryStream(_content), "application/pdf", default);
        string token = tokens.Create(StorageTestData.BlobPath, _oneHour);

        var result = downloads.Open(token + "x");

        result.IsFound.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:BR-D-03")]
    public void Open_ValidTokenOfAFileNeverStored_ReturnsNothing()
    {
        var (downloads, tokens, _) = Create(StorageTestData.Local(_root));
        string token = tokens.Create(StorageTestData.BlobPath, _oneHour);

        var result = downloads.Open(token);

        result.IsFound.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:BR-D-03")]
    public async Task Open_WhenTheProviderIsAzure_ReturnsNothingEvenForAValidToken()
    {
        var local = StorageTestData.Local(_root);
        var (_, tokens, storage) = Create(local);
        await storage.UploadAsync(StorageTestData.BlobPath, new MemoryStream(_content), "application/pdf", default);
        string token = tokens.Create(StorageTestData.BlobPath, _oneHour);
        var (downloads, _, _) = Create(local, StorageTestData.Azure());

        var result = downloads.Open(token);

        result.IsFound.Should().BeFalse();
    }

    private static (LocalDocumentDownloads Downloads, LocalDownloadToken Tokens, LocalFileSystemStorageService Storage)
        Create(IOptions<StorageOptions> local, IOptions<StorageOptions>? effective = null)
    {
        var tokens = new LocalDownloadToken(local, StorageTestData.ClockAt(StorageTestData.Now));
        var storage = new LocalFileSystemStorageService(local, tokens);

        return (new LocalDocumentDownloads(tokens, storage, effective ?? local), tokens, storage);
    }
}
