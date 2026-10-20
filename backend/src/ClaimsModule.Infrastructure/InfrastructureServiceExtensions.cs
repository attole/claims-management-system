using ClaimsModule.Infrastructure.BackgroundJobs;
using ClaimsModule.Infrastructure.Clock;
using ClaimsModule.Infrastructure.Identity;
using ClaimsModule.Infrastructure.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        return services
            .AddClock()
            .AddIdentity()
            .AddStorage()
            .AddBackgroundJobs();
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        return app.UseBackgroundJobsDashboard();
    }
}
