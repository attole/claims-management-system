using ClaimsModule.Application.Abstractions;

namespace ClaimsModule.API.Correlation;

public static class CorrelationServiceExtensions
{
    public static IServiceCollection AddCorrelation(this IServiceCollection services)
    {
        services.AddScoped<ICorrelationContext, CorrelationContext>();

        return services;
    }

    public static IApplicationBuilder UseCorrelation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationMiddleware>();
    }
}
