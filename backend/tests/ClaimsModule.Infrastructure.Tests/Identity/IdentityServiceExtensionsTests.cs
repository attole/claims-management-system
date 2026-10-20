using ClaimsModule.Application.Abstractions;
using ClaimsModule.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Infrastructure.Tests.Identity;

public sealed class IdentityServiceExtensionsTests
{
    [Fact]
    public void AddIdentity_InOneScope_ResolvesTheSameUserForThePortAndTheHolder()
    {
        var provider = new ServiceCollection().AddIdentity().BuildServiceProvider();
        using var scope = provider.CreateScope();

        var port = scope.ServiceProvider.GetRequiredService<ICurrentUser>();
        var holder = scope.ServiceProvider.GetRequiredService<ScopedCurrentUser>();

        port.Should().BeSameAs(holder);
    }
}
