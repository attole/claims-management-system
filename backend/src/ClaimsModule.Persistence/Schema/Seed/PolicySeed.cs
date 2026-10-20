namespace ClaimsModule.Persistence.Schema.Seed;

internal static class PolicySeed
{
    private static readonly SeedPolicy[] _policies =
    [
        new("POL-2024-001001", "Meridian Transport LLC", new(2024, 1, 1), new(2026, 12, 31), ["Vehicle", "Cargo"]),
        new(
            "POL-2024-001002",
            "Harborview Properties Inc",
            new(2024, 6, 1),
            new(2026, 5, 31),
            ["Property", "Liability"]),
        new("POL-2025-002001", "Coastal Builders Group", new(2025, 3, 1), new(2027, 2, 28), ["Property", "Equipment"]),
        new("POL-2025-002002", "Stanton Medical Group", new(2025, 1, 1), new(2026, 12, 31), ["Liability", "Vehicle"]),
        new("POL-2023-000099", "Archived Corp", new(2020, 1, 1), new(2021, 12, 31), ["Property"]),
    ];

    public static object[] Policies()
    {
        var rows = _policies
            .Select(policy => new
            {
                Id = SeedIds.For(policy.PolicyNumber),
                OrganisationId = SeedIds.Organisation,
                policy.PolicyNumber,
                policy.ClientName,
                EffectiveDate = policy.EffectiveDate,
                ExpirationDate = policy.ExpirationDate,
                CreatedAt = SeedIds.SeededAt,
                UserCreated = (Guid?)SeedIds.SystemUser,
                IsDeleted = false,
            })
            .Cast<object>()
            .ToArray();

        return rows;
    }

    public static object[] CoverageTypes()
    {
        var rows = _policies
            .SelectMany(policy => policy.CoverageTypes.Select(name => new
            {
                Id = SeedIds.For($"{policy.PolicyNumber}/{name}"),
                PolicyId = SeedIds.For(policy.PolicyNumber),
                OrganisationId = SeedIds.Organisation,
                Name = name,
                CreatedAt = SeedIds.SeededAt,
                UserCreated = (Guid?)SeedIds.SystemUser,
            }))
            .Cast<object>()
            .ToArray();

        return rows;
    }

    private sealed record SeedPolicy(
        string PolicyNumber,
        string ClientName,
        DateOnly EffectiveDate,
        DateOnly ExpirationDate,
        string[] CoverageTypes);
}
