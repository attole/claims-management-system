using ClaimsModule.Application.Policies.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers.Requests;

public sealed record SearchPoliciesRequest
{
    [FromQuery(Name = "q")]
    public string? Text { get; init; }

    public int Page { get; init; } = PagingDefaults.Page;

    public int PageSize { get; init; } = PagingDefaults.PageSize;

    public ListPoliciesQuery ToQuery()
    {
        return new ListPoliciesQuery(Text ?? string.Empty, Page, PageSize);
    }
}
