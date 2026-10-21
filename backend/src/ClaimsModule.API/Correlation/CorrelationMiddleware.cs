using ClaimsModule.Application.Abstractions;

namespace ClaimsModule.API.Correlation;

internal sealed class CorrelationMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public Task InvokeAsync(HttpContext context, ICorrelationContext correlation)
    {
        var id = correlation.CorrelationId.ToString();
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = id;

            return Task.CompletedTask;
        });

        return next(context);
    }
}
