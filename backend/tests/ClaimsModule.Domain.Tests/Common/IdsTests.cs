using System.Data.SqlTypes;
using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Tests.Common;

public sealed class IdsTests
{
    private static readonly DateTimeOffset _start = new(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void NextAt_OrdersAscendingByTimeInSqlServerOrder()
    {
        var ids = Enumerable
            .Range(0, 200)
            .Select(offset => Ids.NextAt(_start.AddMilliseconds(offset)))
            .ToList();

        for (int index = 1; index < ids.Count; index++)
        {
            new SqlGuid(ids[index - 1]).CompareTo(new SqlGuid(ids[index])).Should().BeNegative();
        }
    }

    [Fact]
    public void NextAt_ReturnsDistinctIdsForTheSameInstant()
    {
        var ids = Enumerable
            .Range(0, 1000)
            .Select(_ => Ids.NextAt(_start))
            .ToList();

        ids.Distinct().Should().HaveCount(1000);
    }

    [Fact]
    public void Next_StampsTheCurrentTime()
    {
        var before = Ids.NextAt(DateTimeOffset.UtcNow.AddSeconds(-1));
        var after = Ids.NextAt(DateTimeOffset.UtcNow.AddSeconds(1));

        var result = Ids.Next();

        new SqlGuid(before).CompareTo(new SqlGuid(result)).Should().BeNegative();
        new SqlGuid(result).CompareTo(new SqlGuid(after)).Should().BeNegative();
    }
}
