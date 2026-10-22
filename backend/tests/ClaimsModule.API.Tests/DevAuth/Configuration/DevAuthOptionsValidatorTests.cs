using ClaimsModule.API.DevAuth.Configuration;

namespace ClaimsModule.API.Tests.DevAuth.Configuration;

public sealed class DevAuthOptionsValidatorTests
{
    private static readonly DevUserOptions _user = new()
    {
        Id = Guid.Parse("c5000000-0000-4000-8000-000000000001"),
        Name = "Test Handler",
        Role = "Handler",
    };

    private readonly DevAuthOptionsValidator _validator = new();

    [Fact]
    public void Validate_WhenDisabled_SucceedsWithoutAnySetting()
    {
        var result = _validator.Validate(null, new DevAuthOptions());

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_EnabledWithEverythingSet_Succeeds()
    {
        var result = _validator.Validate(null, CreateEnabled());

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_EnabledWithoutAnOrganisation_Fails()
    {
        var result = _validator.Validate(null, CreateEnabled(organisationId: Guid.Empty));

        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_EnabledWithoutUsers_Fails()
    {
        var result = _validator.Validate(null, CreateEnabled(users: []));

        result.Failed.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(DevAuthOptionsValidator.MaxTokenLifetimeMinutes + 1)]
    public void Validate_EnabledWithALifetimeOutsideTheRange_Fails(int lifetimeMinutes)
    {
        var result = _validator.Validate(null, CreateEnabled(lifetimeMinutes: lifetimeMinutes));

        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_EnabledWithTheLongestLifetime_Succeeds()
    {
        var result = _validator.Validate(
            null,
            CreateEnabled(lifetimeMinutes: DevAuthOptionsValidator.MaxTokenLifetimeMinutes));

        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("Boss")]
    [InlineData("")]
    [InlineData("7")]
    [InlineData("1")]
    [InlineData("Handler, Supervisor")]
    [InlineData(" Handler")]
    [InlineData("handler")]
    public void Validate_EnabledWithAnUnknownRole_Fails(string role)
    {
        var user = new DevUserOptions { Id = _user.Id, Name = _user.Name, Role = role };

        var result = _validator.Validate(null, CreateEnabled(users: [user]));

        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_EnabledWithAUserWithoutAnId_Fails()
    {
        var user = new DevUserOptions { Id = Guid.Empty, Name = _user.Name, Role = _user.Role };

        var result = _validator.Validate(null, CreateEnabled(users: [user]));

        result.Failed.Should().BeTrue();
    }

    private static DevAuthOptions CreateEnabled(
        Guid? organisationId = null,
        IReadOnlyList<DevUserOptions>? users = null,
        int lifetimeMinutes = 60)
    {
        return new DevAuthOptions
        {
            Enabled = true,
            OrganisationId = organisationId ?? Guid.Parse("c5000000-0000-4000-8000-000000000002"),
            Users = users ?? [_user],
            TokenLifetimeMinutes = lifetimeMinutes,
        };
    }
}
