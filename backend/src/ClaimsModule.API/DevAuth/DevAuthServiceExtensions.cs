using ClaimsModule.API.DevAuth.Configuration;
using Microsoft.Extensions.Options;

namespace ClaimsModule.API.DevAuth;

public static class DevAuthServiceExtensions
{
    public static IServiceCollection AddDevAuth(this IServiceCollection services)
    {
        services
            .AddOptions<DevAuthOptions>()
            .BindConfiguration(DevAuthOptions.SectionName)
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<DevAuthOptions>>(provider =>
            new DevAuthOptionsValidator(!provider.GetRequiredService<IHostEnvironment>().IsDevelopment()));
        services.AddSingleton<DevTokenIssuer>();

        return services;
    }
}
