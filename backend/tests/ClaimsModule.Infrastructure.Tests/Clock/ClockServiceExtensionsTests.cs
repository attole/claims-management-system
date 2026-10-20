using ClaimsModule.Application.Abstractions;
using ClaimsModule.Infrastructure.Clock;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Infrastructure.Tests.Clock;

public sealed class ClockServiceExtensionsTests
{
    [Fact]
    public void AddClock_Registered_ResolvesTheSystemClock()
    {
        var provider = new ServiceCollection().AddClock().BuildServiceProvider();

        var result = provider.GetRequiredService<IClock>();

        result.Should().BeOfType<SystemClock>();
    }
}
