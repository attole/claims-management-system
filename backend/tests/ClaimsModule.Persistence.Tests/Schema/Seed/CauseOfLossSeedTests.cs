using ClaimsModule.Persistence.Schema.Seed;
using ClaimsModule.Persistence.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Tests.Schema.Seed;

public sealed class CauseOfLossSeedTests
{
    [Fact]
    [Trait("Rule", "S:COL-Seed")]
    public void Codes_AfterModelCreation_AreTenActiveCodesOfTheSeedOrganisation()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();

        var result = context.CauseOfLossCodes.IgnoreQueryFilters().Where(code => code.IsActive).ToList();

        result.Should().HaveCount(10).And.OnlyContain(code => code.OrganisationId == SeedIds.Organisation);
    }
}
