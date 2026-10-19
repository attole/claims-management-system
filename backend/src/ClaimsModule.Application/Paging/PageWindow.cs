namespace ClaimsModule.Application.Paging;

public sealed record PageWindow(int Page, int PageSize)
{
    public int Skip => (Page - 1) * PageSize;

    public static PageWindow From(IPagedQuery query)
    {
        return new PageWindow(query.Page, query.PageSize);
    }
}
