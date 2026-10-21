using System.Security.Claims;
using ClaimsModule.API.Pipeline.Identity;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.API.Tests.Pipeline.Identity;

public sealed class TokenUserTests
{
    private static readonly Guid _id = Guid.Parse("c3000000-0000-4000-8000-000000000001");
    private static readonly Guid _organisationId = Guid.Parse("c3000000-0000-4000-8000-000000000002");

    [Fact]
    public void CanRead_CompleteClaims_ReturnsTheUser()
    {
        var principal = CreatePrincipal(_id.ToString(), _organisationId.ToString(), "Supervisor");

        bool result = TokenUser.CanRead(principal, out var user);

        result.Should().BeTrue();
        user.Should().Be(new TokenUser(_id, _organisationId, UserRole.From(UserRoleKind.Supervisor)));
    }

    [Theory]
    [InlineData("", "c3000000-0000-4000-8000-000000000002", "Handler")]
    [InlineData("not-a-guid", "c3000000-0000-4000-8000-000000000002", "Handler")]
    [InlineData("00000000-0000-0000-0000-000000000000", "c3000000-0000-4000-8000-000000000002", "Handler")]
    [InlineData("c3000000-0000-4000-8000-000000000001", "", "Handler")]
    [InlineData("c3000000-0000-4000-8000-000000000001", "00000000-0000-0000-0000-000000000000", "Handler")]
    [InlineData("c3000000-0000-4000-8000-000000000001", "c3000000-0000-4000-8000-000000000002", "")]
    [InlineData("c3000000-0000-4000-8000-000000000001", "c3000000-0000-4000-8000-000000000002", "Boss")]
    [InlineData("c3000000-0000-4000-8000-000000000001", "c3000000-0000-4000-8000-000000000002", "7")]
    [InlineData("c3000000-0000-4000-8000-000000000001", "c3000000-0000-4000-8000-000000000002", "1")]
    [InlineData("c3000000-0000-4000-8000-000000000001", "c3000000-0000-4000-8000-000000000002", "Handler, Supervisor")]
    [InlineData("c3000000-0000-4000-8000-000000000001", "c3000000-0000-4000-8000-000000000002", " Handler")]
    [InlineData("c3000000-0000-4000-8000-000000000001", "c3000000-0000-4000-8000-000000000002", "handler")]
    public void CanRead_AnIncompleteOrInvalidClaim_ReturnsFalse(string subject, string organisation, string role)
    {
        var principal = CreatePrincipal(subject, organisation, role);

        bool result = TokenUser.CanRead(principal, out _);

        result.Should().BeFalse();
    }

    [Fact]
    public void CanRead_AMissingClaim_ReturnsFalse()
    {
        var claims = new List<Claim> { new(IdentityClaims.Subject, _id.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        bool result = TokenUser.CanRead(principal, out _);

        result.Should().BeFalse();
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
