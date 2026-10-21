using ClaimsModule.API.Errors;
using ClaimsModule.Infrastructure.Identity;

namespace ClaimsModule.API.Pipeline.Identity;

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
            await ErrorBody.Unauthorized().WriteAsync(context, context.RequestAborted);

            return;
        }

        current.Set(user.Id, user.OrganisationId, user.Role);

        await next(context);
    }
}
