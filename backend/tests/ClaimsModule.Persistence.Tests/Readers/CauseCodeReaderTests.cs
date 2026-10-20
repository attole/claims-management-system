using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Persistence.Readers;
using ClaimsModule.Persistence.Tests.Support;
using ClaimsModule.Persistence.Tests.Support.CauseOfLoss;
using ClaimsModule.Persistence.Tests.Support.Mapping;

namespace ClaimsModule.Persistence.Tests.Readers;

public sealed class CauseCodeReaderTests
{
    [Fact]
    [Trait("Rule", "S:BR-C-05")]
    public async Task GetActiveAsync_InactiveCode_IsNotActive()
    {
        using var database = new TestDatabase();
        Seed(database, "HAIL", PerilCategory.Weather, true);
        Seed(database, "OLD", PerilCategory.Weather, false);
        using var context = database.CreateContext();
        var reader = new CauseCodeReader(context, TestMapper.Create());

        var result = await reader.GetActiveAsync(CancellationToken.None);

        result.Active.Should().Equal("HAIL");
    }

    [Fact]
    public async Task GetActiveAsync_WithoutActiveCodes_Throws()
    {
        using var database = new TestDatabase();
        using var context = database.CreateContext();
        var reader = new CauseCodeReader(context, TestMapper.Create());

        Func<Task<CauseCodes>> act = () => reader.GetActiveAsync(CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ListActiveAsync_NoPerils_ReturnsEveryActiveCodeInDisplayOrder()
    {
        using var database = new TestDatabase();
        Seed(database, "THEFT", PerilCategory.Crime, true, sortOrder: 2);
        Seed(database, "HAIL", PerilCategory.Weather, true, sortOrder: 1);
        Seed(database, "OLD", PerilCategory.Weather, false, sortOrder: 0);
        using var context = database.CreateContext();
        var reader = new CauseCodeReader(context, TestMapper.Create());

        var result = await reader.ListActiveAsync([], CancellationToken.None);

        result.Select(code => code.Code).Should().Equal("HAIL", "THEFT");
        result[0].PerilCategory.Should().Be("Weather");
    }

    [Fact]
    public async Task ListActiveAsync_Peril_ReturnsOnlyCodesOfThatPeril()
    {
        using var database = new TestDatabase();
        Seed(database, "THEFT", PerilCategory.Crime, true);
        Seed(database, "HAIL", PerilCategory.Weather, true);
        using var context = database.CreateContext();
        var reader = new CauseCodeReader(context, TestMapper.Create());

        var result = await reader.ListActiveAsync([PerilCategory.Crime], CancellationToken.None);

        result.Select(code => code.Code).Should().Equal("THEFT");
    }

    private static void Seed(TestDatabase database, string code, PerilCategory peril, bool isActive, int sortOrder = 0)
    {
        using var context = database.CreateContext();
        var cause = TestCauseCodes.Create(TestDatabase.OrganisationId, code, peril, isActive);
        context.CauseOfLossCodes.Add(cause);
        context.Entry(cause).Property("SortOrder").CurrentValue = sortOrder;
        context.SaveChanges();
    }
}
