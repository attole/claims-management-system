using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.Results;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Policies.Queries;

public sealed record SearchPoliciesQuery(string Text, int Page, int PageSize)
    : IQuery<Result<PagedResult<PolicyDto>>>, IPagedQuery;

public sealed class SearchPoliciesQueryValidator : PagedQueryValidator<SearchPoliciesQuery>
{
    public SearchPoliciesQueryValidator()
    {
        RuleFor(query => query.Text).NotNull().MaximumLength(PolicyFieldLengths.SearchText);
    }
}

public sealed class SearchPoliciesQueryHandler(IPolicyReader policies)
    : IRequestHandler<SearchPoliciesQuery, Result<PagedResult<PolicyDto>>>
{
    public async Task<Result<PagedResult<PolicyDto>>> Handle(
        SearchPoliciesQuery query,
        CancellationToken cancellationToken)
    {
        var found = await policies.SearchAsync(query.Text, PageWindow.From(query), cancellationToken);

        return Result<PagedResult<PolicyDto>>.Success(found);
    }
}
