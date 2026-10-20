namespace ClaimsModule.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "";

    public StorageProvider StorageProvider { get; init; } = StorageProvider.LocalFileSystem;

    public string AzureConnectionString { get; init; } = string.Empty;

    public string LocalRoot { get; init; } = "uploads";

    public string LocalBaseUrl { get; init; } = string.Empty;

    public string LocalSigningKey { get; init; } = string.Empty;
}
