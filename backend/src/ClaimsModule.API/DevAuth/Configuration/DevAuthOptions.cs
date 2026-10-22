namespace ClaimsModule.API.DevAuth.Configuration;

public sealed class DevAuthOptions
{
    public const string SectionName = "DevAuth";

    public bool Enabled { get; init; } = false;

    public Guid OrganisationId { get; init; } = Guid.Empty;

    public int TokenLifetimeMinutes { get; init; } = 480;

    public IReadOnlyList<DevUserOptions> Users { get; init; } = [];
}
