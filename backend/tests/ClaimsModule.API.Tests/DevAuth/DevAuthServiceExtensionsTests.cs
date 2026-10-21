using ClaimsModule.API.Tests.Support;
using Microsoft.Extensions.Options;

namespace ClaimsModule.API.Tests.DevAuth;

[Collection(ApiCollection.Name)]
public sealed class DevAuthServiceExtensionsTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    public void AddDevAuth_EnabledWithAnInvalidUser_StopsTheApplicationAtStartup()
    {
        var settings = new Dictionary<string, string?> { ["DevAuth:Users:0:Role"] = "Boss" };
        using var broken = _factory.WithSettings(settings);

        var act = () => broken.CreateClient();

        act.Should().Throw<OptionsValidationException>().WithMessage("*DevAuth*");
    }
}
