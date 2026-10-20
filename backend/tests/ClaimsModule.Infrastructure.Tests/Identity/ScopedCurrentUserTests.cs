using ClaimsModule.Domain.Roles;
using ClaimsModule.Infrastructure.Identity;

namespace ClaimsModule.Infrastructure.Tests.Identity;

public sealed class ScopedCurrentUserTests
{
    [Fact]
    public void New_HasEmptyIdsAndTheLowestRole()
    {
        var user = new ScopedCurrentUser();

        user.Id.Should().BeEmpty();
        user.OrganisationId.Should().BeEmpty();
        user.Role.Marker.Should().Be(UserRoleKind.Handler);
    }

    [Fact]
    public void Set_ReplacesTheIdentity()
    {
        var user = new ScopedCurrentUser();
        var id = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        user.Set(id, organisationId, UserRole.From(UserRoleKind.Manager));

        user.Id.Should().Be(id);
        user.OrganisationId.Should().Be(organisationId);
        user.Role.Marker.Should().Be(UserRoleKind.Manager);
    }
}
