using System.Net.Http.Headers;
using ClaimsModule.API.Tests.Support.TestEndpoints;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.Results;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ClaimsModule.API.Tests.Support;

public sealed class ApiFactory(ApiFixture database, bool isDevAuthEnabled = true) : WebApplicationFactory<Program>
{
    public static readonly Guid SeedOrganisationId = Guid.Parse("3a44da3d-69e0-b835-4a7c-2859c519e664");
    public static readonly Guid HandlerId = Guid.Parse("b2000000-0000-4000-8000-000000000001");
    public static readonly Guid SupervisorId = Guid.Parse("b2000000-0000-4000-8000-000000000002");
    public static readonly Guid ManagerId = Guid.Parse("b2000000-0000-4000-8000-000000000003");

    public RecordingPostingScheduler Scheduler { get; } = new();

    public WebApplicationFactory<Program> WithSettings(IReadOnlyDictionary<string, string?> settings)
    {
        return WithWebHostBuilder(builder => builder.ConfigureAppConfiguration(
            (_, configuration) => configuration.AddInMemoryCollection(settings)));
    }

    public HttpClient CreateClientFor(Guid userId, Guid organisationId, string role)
    {
        var client = CreateClient();
        var token = TestTokens.Create(userId, organisationId, role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(Settings()));
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IPostingScheduler>();
            services.AddSingleton<IPostingScheduler>(Scheduler);
            services.RemoveAll<IRecurringJobManager>();
            services.AddSingleton(Mock.Of<IRecurringJobManager>());
            services.RemoveAll<IHostedService>();
            services.AddTransient<IRequestHandler<CannedRequest<int>, Result<int>>, CannedRequestHandler<int>>();
            services.AddTransient<
                IRequestHandler<CannedRequest<PagedResult<string>>, Result<PagedResult<string>>>,
                CannedRequestHandler<PagedResult<string>>>();
            services.AddControllers().AddApplicationPart(typeof(TestController).Assembly);
        });
    }

    private Dictionary<string, string?> Settings()
    {
        return new Dictionary<string, string?>
        {
            ["ConnectionStrings:ClaimsDb"] = database.ConnectionString,
            ["Jwt:Issuer"] = TestTokens.Issuer,
            ["Jwt:Audience"] = TestTokens.Audience,
            ["Jwt:SigningKey"] = TestTokens.SigningKey,
            ["Cors:AllowedOrigins:0"] = "http://localhost:4200",
            ["LocalBaseUrl"] = "http://localhost:5000",
            ["LocalSigningKey"] = "test-local-signing-key-0123456789",
            ["DevAuth:Enabled"] = isDevAuthEnabled.ToString(),
            ["DevAuth:OrganisationId"] = SeedOrganisationId.ToString(),
            ["DevAuth:Users:0:Id"] = HandlerId.ToString(),
            ["DevAuth:Users:0:Name"] = "Test Handler",
            ["DevAuth:Users:0:Role"] = "Handler",
            ["DevAuth:Users:1:Id"] = SupervisorId.ToString(),
            ["DevAuth:Users:1:Name"] = "Test Supervisor",
            ["DevAuth:Users:1:Role"] = "Supervisor",
            ["DevAuth:Users:2:Id"] = ManagerId.ToString(),
            ["DevAuth:Users:2:Name"] = "Test Manager",
            ["DevAuth:Users:2:Role"] = "Manager",
        };
    }
}
