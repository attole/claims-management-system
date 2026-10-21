using ClaimsModule.Application.Claims.Queries;
using ClaimsModule.Domain.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers.Requests;

public sealed record ListClaimsRequest
{
    public int Page { get; init; } = PagingDefaults.Page;

    public int PageSize { get; init; } = PagingDefaults.PageSize;

    [FromQuery(Name = "status")]
    public ClaimStateKind[] Statuses { get; init; } = [];

    public DateOnly? DateFrom { get; init; }

    public DateOnly? DateTo { get; init; }

    public Guid AssignedHandlerId { get; init; } = Guid.Empty;

    public string? CauseOfLossCode { get; init; }

    public Guid PolicyId { get; init; } = Guid.Empty;

    public string? Search { get; init; }

    public ListClaimsQuery ToQuery()
    {
        return new ListClaimsQuery(Page, PageSize)
        {
            Statuses = Statuses,
            DateFrom = DateFrom,
            DateTo = DateTo,
            AssignedHandlerId = AssignedHandlerId,
            CauseOfLossCode = CauseOfLossCode ?? string.Empty,
            PolicyId = PolicyId,
            Search = Search ?? string.Empty,
        };
    }
}
