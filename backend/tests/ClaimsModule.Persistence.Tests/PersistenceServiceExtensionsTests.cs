using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Persistence.Tests;

public sealed class PersistenceServiceExtensionsTests
{
    [Fact]
    public void AddPersistence_MissingConnectionString_FailsTheOptionsValidation()
    {
        var provider = CreateProvider([]);

        Func<PersistenceOptions> act = () => provider.GetRequiredService<IOptions<PersistenceOptions>>().Value;

        act.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void AddPersistence_ConnectionString_IsBoundFromTheConnectionStringsSection()
    {
        var provider = CreateProvider(new Dictionary<string, string?> { ["ConnectionStrings:ClaimsDb"] = "Server=x" });

        var result = provider.GetRequiredService<IOptions<PersistenceOptions>>().Value;

        result.ClaimsDb.Should().Be("Server=x");
    }

    private static ServiceProvider CreateProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddPersistence();

        return services.BuildServiceProvider();
    }
}
