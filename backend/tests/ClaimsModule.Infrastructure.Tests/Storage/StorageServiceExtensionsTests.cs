using ClaimsModule.Application.Abstractions;
using ClaimsModule.Infrastructure.Clock;
using ClaimsModule.Infrastructure.Storage;
using ClaimsModule.Infrastructure.Storage.Azure;
using ClaimsModule.Infrastructure.Storage.Local;
using ClaimsModule.Infrastructure.Tests.Support;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.Tests.Storage;

public sealed class StorageServiceExtensionsTests
{
    [Fact]
    [Trait("Rule", "S:BR-D-03")]
    public void AddStorage_NoProviderSet_UsesTheLocalFileSystem()
    {
        var provider = CreateProvider(LocalSettings());

        var result = provider.GetRequiredService<IStorageService>();

        result.Should().BeOfType<LocalFileSystemStorageService>();
    }

    [Fact]
    [Trait("Rule", "S:BR-D-03")]
    public void AddStorage_LocalProvider_RegistersTheLocalServiceAsItself()
    {
        var provider = CreateProvider(LocalSettings());

        var result = provider.GetRequiredService<LocalFileSystemStorageService>();

        result.Should().BeSameAs(provider.GetRequiredService<IStorageService>());
    }

    [Fact]
    [Trait("Rule", "S:BR-D-01")]
    public void AddStorage_AzureProvider_UsesAzureBlobStorage()
    {
        var settings = new Dictionary<string, string?>
        {
            ["StorageProvider"] = "AzureBlob",
            ["AzureConnectionString"] = StorageTestData.AzureConnectionString,
        };
        var provider = CreateProvider(settings);

        var result = provider.GetRequiredService<IStorageService>();

        result.Should().BeOfType<AzureBlobStorageService>();
    }

    [Fact]
    [Trait("Rule", "S:BR-D-03")]
    public void AddStorage_LocalWithoutItsSettings_FailsTheOptionsValidation()
    {
        var provider = CreateProvider([]);

        Func<StorageOptions> act = () => provider.GetRequiredService<IOptions<StorageOptions>>().Value;

        act.Should().Throw<OptionsValidationException>();
    }

    private static Dictionary<string, string?> LocalSettings()
    {
        return new Dictionary<string, string?>
        {
            ["LocalBaseUrl"] = StorageTestData.BaseUrl,
            ["LocalSigningKey"] = StorageTestData.SigningKey,
        };
    }

    private static ServiceProvider CreateProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddClock();
        services.AddStorage();

        return services.BuildServiceProvider();
    }
}
