using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace ClaimsModule.API.Pipeline.Cors;

public static class FrontendCorsServiceExtensions
{
    public const string FrontendPolicyName = "Frontend";

    public static IServiceCollection AddFrontendCors(this IServiceCollection services)
    {
        services
            .AddOptions<FrontendCorsOptions>()
            .BindConfiguration(FrontendCorsOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddCors();

        services
            .AddOptions<CorsOptions>()
            .Configure<IOptions<FrontendCorsOptions>>(
                (cors, frontend) => cors.AddPolicy(FrontendPolicyName, policy => policy
                    .WithOrigins(frontend.Value.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()));

        return services;
    }
}
