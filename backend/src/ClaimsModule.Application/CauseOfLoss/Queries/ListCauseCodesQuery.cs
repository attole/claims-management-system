using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.CauseOfLoss;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.CauseOfLoss.Queries;

public sealed record ListCauseCodesQuery : IQuery<Result<IReadOnlyList<CauseCodeDto>>>
{
    public PerilCategory? PerilCategory { get; init; }

    public IReadOnlyList<PerilCategory> ToPerils()
    {
        IReadOnlyList<PerilCategory> perils = PerilCategory.HasValue ? [PerilCategory.Value] : [];

        return perils;
    }
}

public sealed class ListCauseCodesQueryValidator : AbstractValidator<ListCauseCodesQuery>
{
    public ListCauseCodesQueryValidator()
    {
        RuleFor(query => query.PerilCategory).IsInEnum().When(query => query.PerilCategory.HasValue);
    }
}

public sealed class ListCauseCodesQueryHandler(ICauseCodeReader causes)
    : IRequestHandler<ListCauseCodesQuery, Result<IReadOnlyList<CauseCodeDto>>>
{
    public async Task<Result<IReadOnlyList<CauseCodeDto>>> Handle(
        ListCauseCodesQuery query,
        CancellationToken cancellationToken)
    {
        var codes = await causes.ListActiveAsync(query.ToPerils(), cancellationToken);

        return Result<IReadOnlyList<CauseCodeDto>>.Success(codes);
    }
}
