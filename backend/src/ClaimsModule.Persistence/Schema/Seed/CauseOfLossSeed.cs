using ClaimsModule.Domain.CauseOfLoss;

namespace ClaimsModule.Persistence.Schema.Seed;

internal static class CauseOfLossSeed
{
    private static readonly SeedCode[] _codes =
    [
        new("COL-FIRE", "Fire", PerilCategory.Property),
        new("COL-FLOOD", "Flood", PerilCategory.Weather),
        new("COL-THEFT", "Theft", PerilCategory.Crime),
        new("COL-VEH-COL", "Vehicle Collision", PerilCategory.Auto),
        new("COL-VEH-COMP", "Vehicle Comprehensive", PerilCategory.Auto),
        new("COL-LIAB", "Third Party Liability", PerilCategory.Liability),
        new("COL-EQUIP", "Equipment Breakdown", PerilCategory.Equipment),
        new("COL-WIND", "Wind / Storm", PerilCategory.Weather),
        new("COL-INJURY", "Bodily Injury", PerilCategory.Liability),
        new("COL-OTHER", "Other / Unknown", PerilCategory.General),
    ];

    public static object[] Codes()
    {
        var rows = _codes
            .Select((code, index) => new
            {
                Id = SeedIds.For(code.Code),
                OrganisationId = SeedIds.Organisation,
                code.Code,
                code.Name,
                code.PerilCategory,
                IsActive = true,
                SortOrder = index + 1,
                CreatedAt = SeedIds.SeededAt,
                UserCreated = (Guid?)SeedIds.SystemUser,
                IsDeleted = false,
            })
            .Cast<object>()
            .ToArray();

        return rows;
    }

    private sealed record SeedCode(string Code, string Name, PerilCategory PerilCategory);
}
