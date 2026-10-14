using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Infrastructure.Database;

internal static class DatabaseServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services
            .AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
