using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.Claims;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries;

public sealed record ListClaimsQuery(int Page, int PageSize)
    : IQuery<Result<PagedResult<ClaimSummaryDto>>>, IPagedQuery
{
    public ClaimStateKind? Status { get; init; }

    public DateOnly? DateFrom { get; init; }

    public DateOnly? DateTo { get; init; }

    public Guid AssignedHandlerId { get; init; } = Guid.Empty;

    public string CauseOfLossCode { get; init; } = string.Empty;

    public Guid PolicyId { get; init; } = Guid.Empty;

    public string Search { get; init; } = string.Empty;

    public ClaimListFilter ToFilter()
    {
        IReadOnlyList<ClaimStateKind> statuses = Status.HasValue ? [Status.Value] : [];

        return new ClaimListFilter
        {
            Statuses = statuses,
            DateFrom = DateFrom ?? DateOnly.MinValue,
            DateTo = DateTo ?? DateOnly.MaxValue,
            AssignedHandlerId = AssignedHandlerId,
            CauseOfLossCode = CauseOfLossCode ?? string.Empty,
            PolicyId = PolicyId,
            Search = Search ?? string.Empty,
        };
    }
}

public sealed class ListClaimsQueryValidator : PagedQueryValidator<ListClaimsQuery>
{
    private const string DateRangeMessage = "The end date must not be before the start date.";

    public ListClaimsQueryValidator()
    {
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
        RuleFor(query => query.CauseOfLossCode).NotNull().MaximumLength(ClaimFieldLengths.CauseOfLossCode);
        RuleFor(query => query.Search).NotNull().MaximumLength(ClaimFieldLengths.SearchText);
        RuleFor(query => query.DateTo)
            .Must((query, dateTo) => dateTo >= query.DateFrom)
            .When(query => query.DateFrom.HasValue && query.DateTo.HasValue)
            .WithMessage(DateRangeMessage);
    }
}

public sealed class ListClaimsQueryHandler(IClaimReader claims)
    : IRequestHandler<ListClaimsQuery, Result<PagedResult<ClaimSummaryDto>>>
{
    public async Task<Result<PagedResult<ClaimSummaryDto>>> Handle(
        ListClaimsQuery query,
        CancellationToken cancellationToken)
    {
        var page = await claims.ListAsync(query.ToFilter(), PageWindow.From(query), cancellationToken);

        return Result<PagedResult<ClaimSummaryDto>>.Success(page);
    }
}
