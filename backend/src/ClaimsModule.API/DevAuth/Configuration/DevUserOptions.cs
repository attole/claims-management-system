namespace ClaimsModule.API.DevAuth.Configuration;

public sealed class DevUserOptions
{
    public Guid Id { get; init; } = Guid.Empty;

    public string Name { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;
}
