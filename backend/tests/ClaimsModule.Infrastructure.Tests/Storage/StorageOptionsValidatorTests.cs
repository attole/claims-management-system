using ClaimsModule.Infrastructure.Storage;
using ClaimsModule.Infrastructure.Tests.Support;

namespace ClaimsModule.Infrastructure.Tests.Storage;

public sealed class StorageOptionsValidatorTests
{
    private readonly StorageOptionsValidator _validator = new();

    [Fact]
    [Trait("Rule", "S:BR-D-03")]
    public void Validate_LocalWithBaseUrlAndKey_Succeeds()
    {
        var options = new StorageOptions { LocalBaseUrl = StorageTestData.BaseUrl, LocalSigningKey = "key" };

        var result = _validator.Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", "S:BR-D-03")]
    public void Validate_LocalWithoutBaseUrlAndKey_FailsNamingBothKeys()
    {
        var options = new StorageOptions();

        var result = _validator.Validate(null, options);

        result.FailureMessage.Should().Contain("LocalBaseUrl").And.Contain("LocalSigningKey");
    }

    [Fact]
    [Trait("Rule", "S:BR-D-01")]
    public void Validate_AzureWithConnectionString_Succeeds()
    {
        var options = new StorageOptions { StorageProvider = StorageProvider.AzureBlob, AzureConnectionString = "x" };

        var result = _validator.Validate(null, options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", "S:BR-D-01")]
    public void Validate_AzureWithoutConnectionString_FailsNamingTheKey()
    {
        var options = new StorageOptions { StorageProvider = StorageProvider.AzureBlob };

        var result = _validator.Validate(null, options);

        result.FailureMessage.Should().Contain("AzureConnectionString");
    }
}
