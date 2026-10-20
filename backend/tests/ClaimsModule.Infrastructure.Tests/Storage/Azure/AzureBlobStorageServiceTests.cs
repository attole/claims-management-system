using System.Web;
using ClaimsModule.Infrastructure.Storage.Azure;
using ClaimsModule.Infrastructure.Tests.Support;

namespace ClaimsModule.Infrastructure.Tests.Storage.Azure;

public sealed class AzureBlobStorageServiceTests
{
    private readonly AzureBlobStorageService _service = new(
        StorageTestData.Azure(),
        StorageTestData.ClockAt(StorageTestData.Now));

    [Fact]
    [Trait("Rule", "S:BR-D-02")]
    public async Task GetDownloadUrlAsync_Path_ReturnsASasUrlOfTheBlobInItsContainer()
    {
        var result = await _service.GetDownloadUrlAsync(
            StorageTestData.BlobPath,
            TimeSpan.FromHours(1),
            CancellationToken.None);

        new Uri(result).AbsolutePath.Should().Be("/claim-documents/org-1/claim-1/upload_file.pdf");
    }

    [Fact]
    [Trait("Rule", "S:BR-D-02")]
    public async Task GetDownloadUrlAsync_Path_ReturnsAReadOnlyUrl()
    {
        var result = await _service.GetDownloadUrlAsync(
            StorageTestData.BlobPath,
            TimeSpan.FromHours(1),
            CancellationToken.None);

        HttpUtility.ParseQueryString(new Uri(result).Query)["sp"].Should().Be("r");
    }

    [Fact]
    [Trait("Rule", "S:BR-D-02")]
    public async Task GetDownloadUrlAsync_OneHourLifetime_ExpiresOneHourFromNow()
    {
        var result = await _service.GetDownloadUrlAsync(
            StorageTestData.BlobPath,
            TimeSpan.FromHours(1),
            CancellationToken.None);

        HttpUtility.ParseQueryString(new Uri(result).Query)["se"].Should().Be("2026-01-01T13:00:00Z");
    }
}
