using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.Tests.Roles;

public sealed class UserRoleTests
{
    [Theory]
    [InlineData(UserRoleKind.Handler)]
    [InlineData(UserRoleKind.Supervisor)]
    [InlineData(UserRoleKind.Manager)]
    public void From_ReturnsTheRoleOfTheKind(UserRoleKind kind)
    {
        var result = UserRole.From(kind);

        result.Marker.Should().Be(kind);
    }

    [Fact]
    public void From_ForAnUnknownKind_Throws()
    {
        Func<UserRole> act = () => UserRole.From((UserRoleKind)99);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [Trait("Rule", "S:ROLE-Supervisor")]
    [Trait("Rule", "S:ROLE-Manager")]
    [Trait("Rule", "S:ROLE-Handler")]
    [InlineData(UserRoleKind.Handler, UserRoleKind.Handler, true)]
    [InlineData(UserRoleKind.Handler, UserRoleKind.Supervisor, false)]
    [InlineData(UserRoleKind.Handler, UserRoleKind.Manager, false)]
    [InlineData(UserRoleKind.Supervisor, UserRoleKind.Handler, true)]
    [InlineData(UserRoleKind.Supervisor, UserRoleKind.Supervisor, true)]
    [InlineData(UserRoleKind.Supervisor, UserRoleKind.Manager, false)]
    [InlineData(UserRoleKind.Manager, UserRoleKind.Handler, true)]
    [InlineData(UserRoleKind.Manager, UserRoleKind.Supervisor, true)]
    [InlineData(UserRoleKind.Manager, UserRoleKind.Manager, true)]
    public void HasRightsOf_FollowsTheNesting(UserRoleKind actor, UserRoleKind other, bool isExpected)
    {
        var role = UserRole.From(actor);
        var otherRole = UserRole.From(other);

        var result = role.HasRightsOf(otherRole);

        result.Should().Be(isExpected);
    }

    [Theory]
    [Trait("Rule", "S:BR-R-02")]
    [Trait("Rule", "A:BR-R-03")]
    [Trait("Rule", "A:BR-R-04")]
    [InlineData(UserRoleKind.Handler, 1, false)]
    [InlineData(UserRoleKind.Supervisor, 100_000, true)]
    [InlineData(UserRoleKind.Supervisor, 100_001, false)]
    [InlineData(UserRoleKind.Manager, 12_000_000, true)]
    public void CanDecideReserve_FollowsTheTier(UserRoleKind kind, int amount, bool isExpected)
    {
        var role = UserRole.From(kind);

        var result = role.CanDecideReserve(amount);

        result.Should().Be(isExpected);
    }
}
