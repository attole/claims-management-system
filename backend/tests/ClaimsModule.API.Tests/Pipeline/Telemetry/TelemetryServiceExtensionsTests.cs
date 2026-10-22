using ClaimsModule.API.Pipeline.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace ClaimsModule.API.Tests.Pipeline.Telemetry;

public sealed class TelemetryServiceExtensionsTests
{
    private const string ConnectionString =
        "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://example.invalid/";

    [Fact]
    public void AddTelemetry_WithoutAConnectionString_RegistersNoTracing()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddTelemetry(configuration);

        services.Should().NotContain(descriptor => descriptor.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddTelemetry_WithAConnectionString_RegistersTracing()
    {
        var services = new ServiceCollection();
        var settings = new Dictionary<string, string?> { ["Telemetry:ConnectionString"] = ConnectionString };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        services.AddTelemetry(configuration);

        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(TracerProvider));
    }
}
