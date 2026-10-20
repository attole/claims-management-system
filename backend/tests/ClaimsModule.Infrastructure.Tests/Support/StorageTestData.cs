using ClaimsModule.Application.Abstractions;
using ClaimsModule.Infrastructure.Storage;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.Tests.Support;

internal static class StorageTestData
{
    public const string SigningKey = "test-signing-key";
    public const string BaseUrl = "https://localhost:5001/";
    public const string BlobPath = "claim-documents/org-1/claim-1/upload_file.pdf";
    public const string AzureConnectionString = "DefaultEndpointsProtocol=https;AccountName=testaccount;"
        + "AccountKey=dGVzdC1rZXk=;EndpointSuffix=core.windows.net";

    public static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    public static IClock ClockAt(DateTimeOffset now)
    {
        var clock = new Mock<IClock>();
        clock.Setup(source => source.UtcNow).Returns(now);

        return clock.Object;
    }

    public static IOptions<StorageOptions> Local(string root, string signingKey = SigningKey)
    {
        return Options.Create(new StorageOptions
        {
            LocalRoot = root,
            LocalBaseUrl = BaseUrl,
            LocalSigningKey = signingKey,
        });
    }

    public static IOptions<StorageOptions> Azure()
    {
        return Options.Create(new StorageOptions
        {
            StorageProvider = StorageProvider.AzureBlob,
            AzureConnectionString = AzureConnectionString,
        });
    }

    public static string NewRoot()
    {
        return Path.Combine(Path.GetTempPath(), "claims-storage-tests", Guid.NewGuid().ToString("N"));
    }
}
