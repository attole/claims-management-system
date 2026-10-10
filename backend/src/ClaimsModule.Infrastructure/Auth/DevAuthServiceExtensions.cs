using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Infrastructure.Auth;

internal static class DevAuthServiceExtensions
{
    public static IServiceCollection AddDevAuth(this IServiceCollection services)
    {
        services
            .AddOptions<DevAuthOptions>()
            .BindConfiguration(DevAuthOptions.SectionName);

        return services;
    }
}
