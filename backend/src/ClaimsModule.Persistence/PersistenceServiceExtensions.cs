using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.ReserveComponents.Contracts;
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

        services.AddScoped<IReserveComponentRepository, ReserveComponentRepository>();
        services.AddScoped<IClaimDocumentRepository, ClaimDocumentRepository>();
        services.AddScoped<IClaimReserveReader, ClaimReserveReader>();
        services.AddScoped<IClaimDocumentReader, ClaimDocumentReader>();
        services.AddScoped<AuditSoftDeleteInterceptor>();
        services.AddDbContext<ClaimsDbContext>((provider, options) =>
        {
            var connectionString = provider.GetRequiredService<IOptions<PersistenceOptions>>().Value.ClaimsDb;
            var interceptor = provider.GetRequiredService<AuditSoftDeleteInterceptor>();

            options.UseSqlServer(connectionString).AddInterceptors(interceptor);
        });

        return services;
    }
}
