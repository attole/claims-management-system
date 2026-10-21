using System.Security.Claims;
using ClaimsModule.API.Pipeline.Identity;
using ClaimsModule.Domain.Roles;
using ClaimsModule.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;

namespace ClaimsModule.API.Tests.Pipeline.Identity;

public sealed class CurrentUserMiddlewareTests
{
    private static readonly Guid _id = Guid.Parse("c4000000-0000-4000-8000-000000000001");
    private static readonly Guid _organisationId = Guid.Parse("c4000000-0000-4000-8000-000000000002");

    [Fact]
    public async Task InvokeAsync_AuthenticatedUser_FillsTheScopedCurrentUser()
    {
        var current = new ScopedCurrentUser();
        var principal = CreatePrincipal(_id.ToString(), _organisationId.ToString(), "Manager");

        await Invoke(principal, current);

        current.Id.Should().Be(_id);
        current.OrganisationId.Should().Be(_organisationId);
        current.Role.Marker.Should().Be(UserRoleKind.Manager);
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedUser_CallsTheNextMiddleware()
    {
        var principal = CreatePrincipal(_id.ToString(), _organisationId.ToString(), "Handler");

        var (_, nextCalled) = await Invoke(principal, new ScopedCurrentUser());

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_UnauthenticatedRequest_LeavesTheUserEmptyAndContinues()
    {
        var current = new ScopedCurrentUser();

        var (_, nextCalled) = await Invoke(new ClaimsPrincipal(new ClaimsIdentity()), current);

        current.OrganisationId.Should().BeEmpty();
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedTokenWithoutTheOrganisation_Answers401()
    {
        var principal = CreatePrincipal(_id.ToString(), string.Empty, "Handler");

        var (context, nextCalled) = await Invoke(principal, new ScopedCurrentUser());

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        nextCalled.Should().BeFalse();
    }

    private static async Task<(DefaultHttpContext Context, bool NextCalled)> Invoke(
        ClaimsPrincipal principal,
        ScopedCurrentUser current)
    {
        bool nextCalled = false;
        var middleware = new CurrentUserMiddleware(_ =>
        {
            nextCalled = true;

            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext { User = principal };

        await middleware.InvokeAsync(context, current);

        return (context, nextCalled);
    }

    private static ClaimsPrincipal CreatePrincipal(string subject, string organisation, string role)
    {
        var claims = new List<Claim>
        {
            new(IdentityClaims.Subject, subject),
            new(IdentityClaims.Organisation, organisation),
            new(IdentityClaims.Role, role),
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
