using ClaimsModule.Application.Audit.Queries;
using ClaimsModule.Application.Paging;

namespace ClaimsModule.Application.Tests.Paging;

public sealed class PageWindowTests
{
    [Fact]
    public void From_TakesThePageAndTheSizeOfAPagedQuery()
    {
        var query = new ListClaimAuditEntriesQuery(Guid.NewGuid(), 3, 25);

        var result = PageWindow.From(query);

        result.Should().Be(new PageWindow(3, 25));
    }

    [Theory]
    [InlineData(1, 20, 0)]
    [InlineData(2, 20, 20)]
    [InlineData(5, 10, 40)]
    public void Skip_IsThePagesBeforeTimesThePageSize(int page, int pageSize, int expected)
    {
        var request = new PageWindow(page, pageSize);

        request.Skip.Should().Be(expected);
    }
}
