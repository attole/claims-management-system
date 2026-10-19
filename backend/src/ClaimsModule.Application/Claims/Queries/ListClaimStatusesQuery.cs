using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.Claims;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries;

public sealed record ListClaimStatusesQuery : IQuery<Result<IReadOnlyList<ClaimStatusInfoDto>>>;

public sealed class ListClaimStatusesQueryHandler
    : IRequestHandler<ListClaimStatusesQuery, Result<IReadOnlyList<ClaimStatusInfoDto>>>
{
    public Task<Result<IReadOnlyList<ClaimStatusInfoDto>>> Handle(
        ListClaimStatusesQuery query,
        CancellationToken cancellationToken)
    {
        var statuses = Enum.GetValues<ClaimStateKind>()
            .Select(kind => ToInfo(ClaimState.From(kind)))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<ClaimStatusInfoDto>>.Success(statuses));
    }

    private static ClaimStatusInfoDto ToInfo(ClaimState state)
    {
        var next = ClaimTransitions.NextFrom(state)
            .Select(target => target.Name)
            .ToList();

        return new ClaimStatusInfoDto(state.Name, next);
    }
}
