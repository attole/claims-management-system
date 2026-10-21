using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Infrastructure.Storage.Azure;
using ClaimsModule.Infrastructure.Storage.Local;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.Storage;

internal static class StorageServiceExtensions
{
    public static IServiceCollection AddStorage(this IServiceCollection services)
    {
        services
            .AddOptions<StorageOptions>()
            .BindConfiguration(StorageOptions.SectionName)
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<StorageOptions>, StorageOptionsValidator>();
        services.AddSingleton<LocalDownloadToken>();
        services.AddSingleton<LocalFileSystemStorageService>();
        services.AddSingleton<ILocalDocumentDownloads, LocalDocumentDownloads>();
        services.AddSingleton<AzureBlobStorageService>();
        services.AddSingleton<IStorageService>(CreateStorage);

        return services;
    }

    private static IStorageService CreateStorage(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<IOptions<StorageOptions>>().Value;

        return options.StorageProvider switch
        {
            StorageProvider.AzureBlob => provider.GetRequiredService<AzureBlobStorageService>(),
            StorageProvider.LocalFileSystem => provider.GetRequiredService<LocalFileSystemStorageService>(),
            _ => throw new InvalidOperationException($"Unknown storage provider {options.StorageProvider}."),
        };
    }
}
