using ClaimsModule.Infrastructure.Storage.Local;
using ClaimsModule.Infrastructure.Tests.Support;

namespace ClaimsModule.Infrastructure.Tests.Storage.Local;

public sealed class LocalFileSystemStorageServiceTests : IDisposable
{
    private static readonly TimeSpan _oneHour = TimeSpan.FromHours(1);
    private static readonly byte[] _content = [1, 2, 3, 4];
    private static readonly DateTimeOffset _later = DateTimeOffset.UtcNow.AddMinutes(10);
    private static readonly DateTimeOffset _earlier = DateTimeOffset.UtcNow.AddHours(-1);

    private readonly string _root = StorageTestData.NewRoot();
    private readonly LocalDownloadToken _tokens;
    private readonly LocalFileSystemStorageService _service;

    public LocalFileSystemStorageServiceTests()
    {
        var options = StorageTestData.Local(_root);
        _tokens = new LocalDownloadToken(options, StorageTestData.ClockAt(StorageTestData.Now));
        _service = new LocalFileSystemStorageService(options, _tokens);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    [Trait("Rule", "S:BR-D-03")]
    public async Task OpenRead_AfterUploadToANestedPath_ReturnsTheSameBytes()
    {
        await Upload(StorageTestData.BlobPath);

        var result = ReadAll(_service.OpenRead(StorageTestData.BlobPath));

        result.Should().Equal(_content);
    }

    [Fact]
    [Trait("Rule", "S:BR-D-03")]
    public async Task UploadAsync_Content_IsStoredUnderTheRootWithoutTheContainerFolder()
    {
        await Upload(StorageTestData.BlobPath);

        var result = File.Exists(Path.Combine(_root, "org-1", "claim-1", "upload_file.pdf"));

        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", "S:BR-D-01")]
    public async Task UploadAsync_PathLeavingTheRoot_Throws()
    {
        Func<Task> act = () => Upload("claim-documents/../outside.pdf");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    [Trait("Rule", "S:BR-D-03")]
    public async Task GetDownloadUrlAsync_BaseUrlWithTrailingSlash_JoinsTheDownloadRoute()
    {
        var result = await _service.GetDownloadUrlAsync(StorageTestData.BlobPath, _oneHour, CancellationToken.None);

        result.Should().StartWith("https://localhost:5001/api/documents/download?token=");
    }

    [Fact]
    [Trait("Rule", "S:BR-D-02")]
    public async Task GetDownloadUrlAsync_Path_ReturnsATokenThatReadsBackAsThatPath()
    {
        var result = await _service.GetDownloadUrlAsync(StorageTestData.BlobPath, _oneHour, CancellationToken.None);

        _tokens.ReadPath(TokenFrom(result)).Should().Be(StorageTestData.BlobPath);
    }

    [Fact]
    public async Task ListAsync_FileOlderThanTheLimit_IsListedWithTheContainerPrefix()
    {
        await Upload(StorageTestData.BlobPath);

        var result = await _service.ListAsync("claim-documents", _later, CancellationToken.None);

        result.Should().Equal(StorageTestData.BlobPath);
    }

    [Fact]
    public async Task ListAsync_FileNewerThanTheLimit_IsNotListed()
    {
        await Upload(StorageTestData.BlobPath);

        var result = await _service.ListAsync("claim-documents", _earlier, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ListAsync_BeforeAnythingWasStored_ReturnsNothing()
    {
        var result = await _service.ListAsync("claim-documents", _later, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_StoredFile_RemovesIt()
    {
        await Upload(StorageTestData.BlobPath);

        await _service.DeleteAsync(StorageTestData.BlobPath, CancellationToken.None);

        File.Exists(Path.Combine(_root, "org-1", "claim-1", "upload_file.pdf")).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_PathNeverStored_DoesNotThrow()
    {
        Func<Task> act = () => _service.DeleteAsync(StorageTestData.BlobPath, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    private static string TokenFrom(string url)
    {
        var query = new Uri(url).Query;

        return Uri.UnescapeDataString(query["?token=".Length..]);
    }

    private static byte[] ReadAll(Stream stream)
    {
        using var source = stream;
        using var buffer = new MemoryStream();
        source.CopyTo(buffer);

        return buffer.ToArray();
    }

    private Task Upload(string path)
    {
        return _service.UploadAsync(path, new MemoryStream(_content), "application/pdf", CancellationToken.None);
    }
}
