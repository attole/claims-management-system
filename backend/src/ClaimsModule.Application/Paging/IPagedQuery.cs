using FluentValidation;

namespace ClaimsModule.Application.Paging;

public interface IPagedQuery
{
    int Page { get; }

    int PageSize { get; }
}

public static class PageLimits
{
    public const int MaxPageSize = 100;
}

public abstract class PagedQueryValidator<TQuery> : AbstractValidator<TQuery>
    where TQuery : IPagedQuery
{
    protected PagedQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, PageLimits.MaxPageSize);
    }
}
