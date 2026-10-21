using AutoMapper;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Paging;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Persistence.Readers;
using ClaimsModule.Persistence.SqlServerTests.Support;

namespace ClaimsModule.Persistence.SqlServerTests.Readers;

[Collection(SqlServerCollection.Name)]
public sealed class ClaimReaderTests(SqlServerFixture database)
{
    [SqlServerFact]
    [Trait("Rule", "S:UI-List")]
    public async Task ListAsync_ForALossDateRange_MatchesWholeUtcDaysIncludingBothEnds()
    {
        var organisationId = Guid.NewGuid();
        var before = LossOn(organisationId, 2, 26, 23, 59);
        var first = LossOn(organisationId, 2, 27, 0, 0);
        var last = LossOn(organisationId, 2, 28, 23, 59);
        var after = LossOn(organisationId, 3, 1, 0, 0);
        using (var setup = database.CreateContext(organisationId))
        {
            setup.Claims.AddRange(before, first, last, after);
            await setup.SaveChangesAsync();
        }

        using var context = database.CreateContext(organisationId);
        var reader = new ClaimReader(context, new Mock<IMapper>().Object);
        var filter = new ClaimListFilter { DateFrom = new DateOnly(2026, 2, 27), DateTo = new DateOnly(2026, 2, 28) };

        var result = await reader.ListAsync(filter, new PageWindow(1, 25), CancellationToken.None);

        result.Items.Select(item => item.Id).Should().BeEquivalentTo([first.Id, last.Id]);
    }

    private static Claim LossOn(Guid organisationId, int month, int day, int hour, int minute)
    {
        var lossDate = new DateTimeOffset(2026, month, day, hour, minute, 0, TimeSpan.Zero);

        return SqlServerClaims.CreateWithLoss(organisationId, lossDate);
    }
}
