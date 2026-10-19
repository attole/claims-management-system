using ClaimsModule.Application.Paging;

namespace ClaimsModule.Application.Tests.Paging;

public sealed class PagedResultTests
{
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(95, 10, 10)]
    public void TotalPages_RoundsUp(int totalCount, int pageSize, int expected)
    {
        var page = new PagedResult<string>([], 1, pageSize, totalCount);

        page.TotalPages.Should().Be(expected);
    }

    [Fact]
    public void TotalPages_WithAZeroPageSize_IsZero()
    {
        var page = new PagedResult<string>([], 1, 0, 5);

        page.TotalPages.Should().Be(0);
    }
}
