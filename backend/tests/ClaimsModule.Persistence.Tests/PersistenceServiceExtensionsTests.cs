using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Persistence.Audit;
using ClaimsModule.Persistence.ClaimNumbers;
using ClaimsModule.Persistence.Readers;
using ClaimsModule.Persistence.Repositories;
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

    [Theory]
    [InlineData(typeof(IClaimRepository), typeof(ClaimRepository))]
    [InlineData(typeof(IReserveComponentRepository), typeof(ReserveComponentRepository))]
    [InlineData(typeof(IClaimDocumentRepository), typeof(ClaimDocumentRepository))]
    [InlineData(typeof(IClaimReader), typeof(ClaimReader))]
    [InlineData(typeof(IPolicyReader), typeof(PolicyReader))]
    [InlineData(typeof(ICauseCodeReader), typeof(CauseCodeReader))]
    [InlineData(typeof(IClaimReserveReader), typeof(ClaimReserveReader))]
    [InlineData(typeof(IClaimDocumentReader), typeof(ClaimDocumentReader))]
    [InlineData(typeof(IClaimNumberGenerator), typeof(ClaimNumberGenerator))]
    [InlineData(typeof(IAuditLogReader), typeof(AuditLogReader))]
    [InlineData(typeof(IAuditLogService), typeof(AuditLogService))]
    [InlineData(typeof(IUnitOfWork), typeof(UnitOfWork))]
    public void AddPersistence_Port_IsImplementedByThePersistenceClass(Type port, Type implementation)
    {
        var services = new ServiceCollection();

        services.AddPersistence();

        services.Should().Contain(descriptor => descriptor.ServiceType == port
            && descriptor.ImplementationType == implementation);
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
