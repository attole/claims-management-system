using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ClaimsModule.API.Tests.Support;

public sealed class KestrelApiFactory(ApiFixture database) : ApiFactory(database)
{
    private IHost _kestrelHost = new HostBuilder().Build();

    public Uri KestrelAddress { get; private set; } = new("http://localhost");

    public HttpClient CreateKestrelClientFor(Guid userId, Guid organisationId, string role)
    {
        _ = Services;
        var client = new HttpClient { BaseAddress = KestrelAddress, Timeout = TimeSpan.FromMinutes(2) };
        var token = TestTokens.Create(userId, organisationId, role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.ExpectContinue = true;

        return client;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var testHost = builder.Build();
        builder.ConfigureWebHost(web => web.UseKestrel(options => options.Listen(IPAddress.Loopback, 0)));
        _kestrelHost = builder.Build();
        _kestrelHost.Start();
        var addresses = _kestrelHost.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
        KestrelAddress = new Uri(addresses!.Addresses.First());
        testHost.Start();

        return testHost;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _kestrelHost.Dispose();
        }

        base.Dispose(disposing);
    }
}
