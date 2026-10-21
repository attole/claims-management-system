using ClaimsModule.Infrastructure.Identity;

namespace ClaimsModule.API.Identity;

internal sealed class CurrentUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ScopedCurrentUser current)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);

            return;
        }

        if (!TokenUser.CanRead(context.User, out var user))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            return;
        }

        current.Set(user.Id, user.OrganisationId, user.Role);

        await next(context);
    }
}
