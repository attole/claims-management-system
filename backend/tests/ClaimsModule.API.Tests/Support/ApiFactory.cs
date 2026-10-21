using System.Net.Http.Headers;
using ClaimsModule.API.Tests.Support.TestEndpoints;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.Roles;
using ClaimsModule.Infrastructure.Identity;
using ClaimsModule.Persistence;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ClaimsModule.API.Tests.Support;

public class ApiFactory(ApiFixture database, bool isDevAuthEnabled = true) : WebApplicationFactory<Program>
{
    public static readonly Guid SeedOrganisationId = Guid.Parse("3a44da3d-69e0-b835-4a7c-2859c519e664");
    public const string SeedCauseOfLossCode = "WATER";
    public const string DevAuthSecret = "test-devauth-secret-0123456789";

    public static readonly Guid HandlerId = Guid.Parse("b2000000-0000-4000-8000-000000000001");
    public static readonly Guid SupervisorId = Guid.Parse("b2000000-0000-4000-8000-000000000002");
    public static readonly Guid ManagerId = Guid.Parse("b2000000-0000-4000-8000-000000000003");

    private readonly string _storageRoot = Path.Combine(Path.GetTempPath(), $"claims-api-test-{Guid.NewGuid():N}");

    public RecordingPostingScheduler Scheduler { get; } = new();

    public string StorageRoot => _storageRoot;

    public WebApplicationFactory<Program> WithSettings(IReadOnlyDictionary<string, string?> settings)
    {
        return WithWebHostBuilder(builder => builder.ConfigureAppConfiguration(
            (_, configuration) => configuration.AddInMemoryCollection(settings)));
    }

    public async Task<Guid> CreateOrganisationAsync()
    {
        var organisationId = Guid.NewGuid();
        string code = $"TEST-{organisationId:N}";
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO CauseOfLossCodes
                (Code, Name, PerilCategory, IsActive, IsDeleted, CreatedAt, SortOrder, OrganisationId)
            VALUES
                ({code}, 'Water damage', 'Property', 1, 0, SYSDATETIMEOFFSET(), 1, {organisationId})
            """);

        return organisationId;
    }

    public async Task<Guid> SeedOpenClaimAsync()
    {
        using var scope = Services.CreateScope();
        var provider = scope.ServiceProvider;
        provider.GetRequiredService<ScopedCurrentUser>()
            .Set(HandlerId, SeedOrganisationId, UserRole.From(UserRoleKind.Handler));
        var context = provider.GetRequiredService<ClaimsDbContext>();
        var policy = await context.Policies.OrderBy(item => item.PolicyNumber).FirstAsync();
        var period = await provider.GetRequiredService<IPolicyReader>().GetPeriodAsync(policy.Id, default);
        var codes = await provider.GetRequiredService<ICauseCodeReader>().GetActiveAsync(default);
        var now = DateTimeOffset.UtcNow;
        var number = ClaimNumber.Create(2026, Random.Shared.Next(1, 9_999_999));
        var input = new CreateClaimInput(SeedOrganisationId, number, now, now, PolicyPeriod.Empty, codes);
        var claim = Claim.Create(input).Value;
        var lossDay = period.StartDate.AddDays(10).ToDateTime(TimeOnly.MinValue);
        var lossDate = new LossDate(new DateTimeOffset(lossDay, TimeSpan.Zero));
        var loss = new LossDetails(lossDate, "Water damage in the kitchen", codes.Active.First());
        var claimant = new PartyDetails(new PartyClaimantRole(), new PartyPersonType())
        {
            FirstName = "Ada",
            LastName = "Stone",
        };
        var riskObject = new RiskObjectDetails(AssetType.Property, "Kitchen");
        claim.LinkPolicy(new LinkPolicyInput(policy.Id, period, codes, now));
        claim.UpdateLossDetails(new UpdateLossDetailsInput(loss, period, codes, now));
        claim.AddRiskObject(new AddRiskObjectInput(riskObject, period, codes, now));
        claim.AssignHandler(new AssignHandlerInput(HandlerId, period, codes, now));
        claim.AddParty(new AddPartyInput(claimant, period, codes, now));
        claim.Open(new ClaimTransitionInput(period, codes, now));
        context.Claims.Add(claim);
        await context.SaveChangesAsync();

        return claim.Id;
    }

    public async Task<(Guid Id, string Number)> SeedClaimAsync(Guid organisationId)
    {
        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ScopedCurrentUser>()
            .Set(Guid.NewGuid(), organisationId, UserRole.From(UserRoleKind.Handler));
        var context = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        var number = ClaimNumber.Create(2026, Random.Shared.Next(1, 9_999_999));
        var now = DateTimeOffset.UtcNow;
        var causeCodes = new CauseCodes([SeedCauseOfLossCode]);
        var input = new CreateClaimInput(organisationId, number, now, now, PolicyPeriod.Empty, causeCodes);
        var claim = Claim.Create(input).Value;
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var period = new PolicyPeriod(Guid.Empty, today.AddYears(-1), today.AddYears(1), DateOnly.MinValue);
        var loss = new LossDetails(new LossDate(now.AddDays(-3)), "Water damage in the kitchen", SeedCauseOfLossCode);
        claim.UpdateLossDetails(new UpdateLossDetailsInput(loss, period, causeCodes, now));
        context.Claims.Add(claim);
        await context.SaveChangesAsync();

        return (claim.Id, number.Value);
    }

    public HttpClient CreateClientFor(Guid userId, Guid organisationId, string role)
    {
        var client = CreateClient();
        var token = TestTokens.Create(userId, organisationId, role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && Directory.Exists(_storageRoot))
        {
            Directory.Delete(_storageRoot, true);
        }
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
            ["LocalRoot"] = _storageRoot,
            ["LocalSigningKey"] = "test-local-signing-key-0123456789",
            ["DevAuth:Enabled"] = isDevAuthEnabled.ToString(),
            ["DevAuth:Secret"] = DevAuthSecret,
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
