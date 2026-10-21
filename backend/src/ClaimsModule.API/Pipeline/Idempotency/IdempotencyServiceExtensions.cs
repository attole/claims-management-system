using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Pipeline.Idempotency;

public static class IdempotencyServiceExtensions
{
    public static IServiceCollection AddIdempotency(this IServiceCollection services)
    {
        services.Configure<MvcOptions>(options => options.Filters.Add<IdempotencyFilter>());

        return services;
    }
}
