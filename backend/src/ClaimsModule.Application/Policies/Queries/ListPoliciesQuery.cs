using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.Results;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Policies.Queries;

public sealed record ListPoliciesQuery(string Text, int Page, int PageSize)
    : IQuery<Result<PagedResult<PolicyDto>>>, IPagedQuery;

public sealed class ListPoliciesQueryValidator : PagedQueryValidator<ListPoliciesQuery>
{
    public ListPoliciesQueryValidator()
    {
        RuleFor(query => query.Text).NotNull().MaximumLength(PolicyFieldLengths.SearchText);
    }
}

public sealed class ListPoliciesQueryHandler(IPolicyReader policies)
    : IRequestHandler<ListPoliciesQuery, Result<PagedResult<PolicyDto>>>
{
    public async Task<Result<PagedResult<PolicyDto>>> Handle(
        ListPoliciesQuery query,
        CancellationToken cancellationToken)
    {
        var found = await policies.SearchAsync(query.Text, PageWindow.From(query), cancellationToken);

        return Result<PagedResult<PolicyDto>>.Success(found);
    }
}
