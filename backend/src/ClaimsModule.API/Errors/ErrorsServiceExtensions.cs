using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Errors;

public static class ErrorsServiceExtensions
{
    private static readonly int[] _errorStatuses =
    [
        StatusCodes.Status401Unauthorized,
        StatusCodes.Status403Forbidden,
        StatusCodes.Status404NotFound,
        StatusCodes.Status409Conflict,
        StatusCodes.Status422UnprocessableEntity,
    ];

    public static IServiceCollection AddErrors(this IServiceCollection services)
    {
        services.PostConfigure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory =
            context => ErrorBody.FromModelState(context.ModelState).ToResult());
        services.AddExceptionHandler<ConcurrencyConflictExceptionHandler>();
        services.Configure<MvcOptions>(options =>
        {
            foreach (var status in _errorStatuses)
            {
                options.Filters.Add(new ProducesResponseTypeAttribute<ErrorBody>(status));
            }
        });

        return services;
    }
}
