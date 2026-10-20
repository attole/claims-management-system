using ClaimsModule.Application.Abstractions;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Persistence.ClaimNumbers;
using ClaimsModule.Persistence.SqlServerTests.Support;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.SqlServerTests.ClaimNumbers;

[Collection(SqlServerCollection.Name)]
public sealed class ClaimNumberGeneratorTests(SqlServerFixture database)
{
    private const int Year = 2026;

    [SqlServerFact]
    [Trait("Rule", "S:CLAIMNO-Atomic")]
    public async Task NextAsync_FirstCallOfAnOrganisation_ReturnsSequenceOne()
    {
        using var context = database.CreateContext(Guid.NewGuid());
        var generator = new ClaimNumberGenerator(context);

        var result = await generator.NextAsync(Year, CancellationToken.None);

        result.Should().Be(ClaimNumber.Create(Year, 1));
    }

    [SqlServerFact]
    [Trait("Rule", "S:CLAIMNO-Atomic")]
    public async Task NextAsync_SecondCallInTheSameTransaction_ReturnsSequenceTwo()
    {
        using var context = database.CreateContext(Guid.NewGuid());
        var generator = new ClaimNumberGenerator(context);
        await generator.NextAsync(Year, CancellationToken.None);

        var result = await generator.NextAsync(Year, CancellationToken.None);

        result.Should().Be(ClaimNumber.Create(Year, 2));
    }

    [SqlServerFact]
    [Trait("Rule", "S:CLAIMNO-Atomic")]
    public async Task NextAsync_AfterARollback_ReusesTheNumber()
    {
        var organisationId = Guid.NewGuid();
        using (var abandoned = database.CreateContext(organisationId))
        {
            await new ClaimNumberGenerator(abandoned).NextAsync(Year, CancellationToken.None);
        }

        using var context = database.CreateContext(organisationId);
        var generator = new ClaimNumberGenerator(context);

        var result = await generator.NextAsync(Year, CancellationToken.None);

        result.Should().Be(ClaimNumber.Create(Year, 1));
    }

    [SqlServerFact]
    [Trait("Rule", "S:CLAIMNO-Atomic")]
    public async Task NextAsync_ParallelRequests_ReturnDistinctNumbersWithoutGaps()
    {
        var organisationId = Guid.NewGuid();
        var requests = Enumerable.Range(0, 8)
            .Select(_ => (Func<Task<ClaimNumber>>)(() => NextAndCommitAsync(organisationId)))
            .ToList();
        var expected = Enumerable.Range(1, 8).Select(sequence => ClaimNumber.Create(Year, sequence)).ToList();

        var result = await Task.WhenAll(requests.Select(request => request()));

        result.Should().BeEquivalentTo(expected);
    }

    [SqlServerFact]
    public async Task NextAsync_CounterAtTheLimit_ReportsAConflict()
    {
        var organisationId = Guid.NewGuid();
        using var context = database.CreateContext(organisationId);
        await context.Database.ExecuteSqlAsync($"""
            INSERT INTO ClaimNumberCounters (OrganisationId, [Year], LastValue)
            VALUES ({organisationId}, {Year}, 9999999)
            """);
        var generator = new ClaimNumberGenerator(context);

        Func<Task> act = () => generator.NextAsync(Year, CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrencyConflictException>();
    }

    private async Task<ClaimNumber> NextAndCommitAsync(Guid organisationId)
    {
        using var context = database.CreateContext(organisationId);
        var generator = new ClaimNumberGenerator(context);
        var number = await generator.NextAsync(Year, CancellationToken.None);

        await context.Database.CommitTransactionAsync();

        return number;
    }
}
