namespace ClaimsModule.Infrastructure.Auth;

public sealed class DevAuthOptions
{
    public const string SectionName = "DevAuth";

    public bool Enabled { get; init; } = false;
}
