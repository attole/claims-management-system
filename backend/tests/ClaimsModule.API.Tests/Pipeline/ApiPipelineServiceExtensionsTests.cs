using ClaimsModule.API.Pipeline;
using ClaimsModule.Application.Abstractions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.API.Tests.Pipeline;

public sealed class ApiPipelineServiceExtensionsTests
{
    [Theory]
    [InlineData(typeof(ICorrelationContext))]
    [InlineData(typeof(IExceptionHandler))]
    public void AddApiPipeline_RegistersTheCrossCuttingServices(Type serviceType)
    {
        var services = new ServiceCollection();

        services.AddApiPipeline(new ConfigurationBuilder().Build());

        services.Should().Contain(descriptor => descriptor.ServiceType == serviceType);
    }
}
