using ClaimsModule.Infrastructure.Auth;
using ClaimsModule.Infrastructure.BackgroundJobs;
using ClaimsModule.Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        return services
            .AddDatabaseOptions()
            .AddBackgroundJobs()
            .AddDevAuth();
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        return app.UseBackgroundJobsDashboard();
    }
}
