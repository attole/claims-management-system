using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Persistence.Audit;
using ClaimsModule.Persistence.ClaimNumbers;
using ClaimsModule.Persistence.Interceptors;
using ClaimsModule.Persistence.Readers;
using ClaimsModule.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClaimsModule.Persistence;

public static class PersistenceServiceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services
            .AddOptions<PersistenceOptions>()
            .BindConfiguration(PersistenceOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<IReserveComponentRepository, ReserveComponentRepository>();
        services.AddScoped<IClaimDocumentRepository, ClaimDocumentRepository>();
        services.AddScoped<IClaimReader, ClaimReader>();
        services.AddScoped<IPolicyReader, PolicyReader>();
        services.AddScoped<ICauseCodeReader, CauseCodeReader>();
        services.AddScoped<IClaimReserveReader, ClaimReserveReader>();
        services.AddScoped<IClaimDocumentReader, ClaimDocumentReader>();
        services.AddScoped<IClaimNumberGenerator, ClaimNumberGenerator>();
        services.AddScoped<IAuditLogReader, AuditLogReader>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddDbContext<ClaimsDbContext>((provider, options) =>
        {
            var connectionString = provider.GetRequiredService<IOptions<PersistenceOptions>>().Value.ClaimsDb;
            var clock = provider.GetRequiredService<IClock>();
            var currentUser = provider.GetRequiredService<ICurrentUser>();

            options.UseSqlServer(connectionString).AddInterceptors(SaveInterceptors.Create(clock, currentUser));
        });

        return services;
    }
}
