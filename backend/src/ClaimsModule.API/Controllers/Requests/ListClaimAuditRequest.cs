using ClaimsModule.Application.Audit.Queries;

namespace ClaimsModule.API.Controllers.Requests;

public sealed record ListClaimAuditRequest
{
    public int Page { get; init; } = PagingDefaults.Page;

    public int PageSize { get; init; } = PagingDefaults.PageSize;

    public ListClaimAuditEntriesQuery ToQuery(Guid claimId)
    {
        return new ListClaimAuditEntriesQuery(claimId, Page, PageSize);
    }
}
