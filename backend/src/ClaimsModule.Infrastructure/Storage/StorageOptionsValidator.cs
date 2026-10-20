using Microsoft.Extensions.Options;

namespace ClaimsModule.Infrastructure.Storage;

internal sealed class StorageOptionsValidator : IValidateOptions<StorageOptions>
{
    public ValidateOptionsResult Validate(string? name, StorageOptions options)
    {
        var missing = RequiredFor(options)
            .Where(setting => setting.Value.Length == 0)
            .Select(setting => setting.Key)
            .ToList();

        return missing.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail($"{string.Join(", ", missing)} required for {options.StorageProvider}");
    }

    private static Dictionary<string, string> RequiredFor(StorageOptions options)
    {
        return options.StorageProvider switch
        {
            StorageProvider.AzureBlob => new Dictionary<string, string>
            {
                [nameof(StorageOptions.AzureConnectionString)] = options.AzureConnectionString,
            },
            _ => new Dictionary<string, string>
            {
                [nameof(StorageOptions.LocalBaseUrl)] = options.LocalBaseUrl,
                [nameof(StorageOptions.LocalSigningKey)] = options.LocalSigningKey,
            },
        };
    }
}
