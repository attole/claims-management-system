using ClaimsModule.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Infrastructure.Clock;

internal static class ClockServiceExtensions
{
    public static IServiceCollection AddClock(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
