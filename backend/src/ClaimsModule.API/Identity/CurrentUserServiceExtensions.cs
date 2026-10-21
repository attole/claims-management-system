namespace ClaimsModule.API.Identity;

public static class CurrentUserServiceExtensions
{
    public static IApplicationBuilder UseCurrentUser(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CurrentUserMiddleware>();
    }
}
