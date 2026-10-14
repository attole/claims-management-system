using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Tests.Common.Roles;

public sealed class UserRoleTests
{
    private static readonly UserRole[] _roles =
    [
        new UserHandlerRole(),
        new UserSupervisorRole(),
        new UserManagerRole(),
    ];

    [Fact]
    public void Roles_CoverEveryKind()
    {
        var result = _roles.Select(role => role.Marker);

        result.Should().BeEquivalentTo(Enum.GetValues<UserRoleKind>());
    }
}
