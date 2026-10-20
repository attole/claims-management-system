using ClaimsModule.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Infrastructure.Tests;

public sealed class InfrastructureServiceExtensionsTests
{
    [Theory]
    [InlineData(typeof(IClock))]
    [InlineData(typeof(IStorageService))]
    public void AddInfrastructure_Port_IsRegistered(Type port)
    {
        var services = new ServiceCollection();

        services.AddInfrastructure();

        services.Should().Contain(descriptor => descriptor.ServiceType == port);
    }
}
