using ClaimsModule.API.Identity;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.API.Tests.Identity;

public sealed class RoleNamesTests
{
    [Theory]
    [InlineData("Handler", UserRoleKind.Handler)]
    [InlineData("Supervisor", UserRoleKind.Supervisor)]
    [InlineData("Manager", UserRoleKind.Manager)]
    public void IsKnown_ExactRoleName_ReturnsTheKind(string value, UserRoleKind expected)
    {
        bool result = RoleNames.IsKnown(value, out var kind);

        result.Should().BeTrue();
        kind.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Boss")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("handler")]
    [InlineData(" Handler")]
    [InlineData("Handler ")]
    [InlineData("Handler, Supervisor")]
    public void IsKnown_AnythingButAnExactName_ReturnsFalse(string value)
    {
        bool result = RoleNames.IsKnown(value, out _);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsKnown_ANullValue_ReturnsFalse()
    {
        bool result = RoleNames.IsKnown(null, out _);

        result.Should().BeFalse();
    }
}
