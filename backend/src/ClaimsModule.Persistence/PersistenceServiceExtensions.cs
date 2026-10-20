using ClaimsModule.Persistence.Interceptors;
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
