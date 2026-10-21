namespace ClaimsModule.API.DevAuth.Configuration;

public sealed class DevAuthOptions
{
    public const string SectionName = "DevAuth";

    public const string SecretHeader = "X-DevAuth-Secret";

    public bool Enabled { get; init; } = false;

    public Guid OrganisationId { get; init; } = Guid.Empty;

    public int TokenLifetimeMinutes { get; init; } = 480;

    public string Secret { get; init; } = string.Empty;

    public IReadOnlyList<DevUserOptions> Users { get; init; } = [];

    public bool HasSecret => Secret.Length > 0;
}
