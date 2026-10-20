using ClaimsModule.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Infrastructure.Identity;

internal static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services.AddScoped<ScopedCurrentUser>();
        services.AddScoped<ICurrentUser>(provider => provider.GetRequiredService<ScopedCurrentUser>());

        return services;
    }
}
