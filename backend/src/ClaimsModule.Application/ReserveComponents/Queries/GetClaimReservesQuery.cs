using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.ReserveComponents.Dtos;
using ClaimsModule.Application.Results;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.ReserveComponents.Queries;

public sealed record GetClaimReservesQuery(Guid ClaimId) : IQuery<Result<ClaimReservesDto>>;

public sealed class GetClaimReservesQueryValidator : AbstractValidator<GetClaimReservesQuery>
{
    public GetClaimReservesQueryValidator()
    {
        RuleFor(query => query.ClaimId).NotEmpty();
    }
}

public sealed class GetClaimReservesQueryHandler(IClaimReader claims, IClaimReserveReader reserves)
    : IRequestHandler<GetClaimReservesQuery, Result<ClaimReservesDto>>
{
    public async Task<Result<ClaimReservesDto>> Handle(
        GetClaimReservesQuery query,
        CancellationToken cancellationToken)
    {
        bool isKnown = await claims.ExistsAsync(query.ClaimId, cancellationToken);

        if (!isKnown)
        {
            return Result<ClaimReservesDto>.Failure([ClaimErrors.ClaimNotFound(query.ClaimId)]);
        }

        var found = await reserves.GetReservesAsync(query.ClaimId, cancellationToken);

        return Result<ClaimReservesDto>.Success(found);
    }
}
