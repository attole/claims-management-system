using ClaimsModule.Domain.Common;
using System.Data.SqlTypes;

namespace ClaimsModule.Domain.Tests.Common;

public sealed class IdsTests
{
    private static readonly DateTimeOffset _start = new(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Next_OrdersAscendingByTimeInSqlServerOrder()
    {
        List<Guid> ids = Enumerable
            .Range(0, 200)
            .Select(offset => Ids.Next(_start.AddMilliseconds(offset)))
            .ToList();

        for (int index = 1; index < ids.Count; index++)
        {
            new SqlGuid(ids[index - 1]).CompareTo(new SqlGuid(ids[index])).Should().BeNegative();
        }
    }

    [Fact]
    public void Next_ReturnsDistinctIdsForTheSameInstant()
    {
        List<Guid> ids = Enumerable
            .Range(0, 1000)
            .Select(_ => Ids.Next(_start))
            .ToList();

        ids.Distinct().Should().HaveCount(1000);
    }
}
