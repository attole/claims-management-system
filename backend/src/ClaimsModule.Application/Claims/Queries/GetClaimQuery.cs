using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.ClaimDocuments;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.Results;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries;

public sealed record GetClaimQuery(Guid ClaimId) : IQuery<Result<ClaimDetailDto>>;

public sealed class GetClaimQueryValidator : AbstractValidator<GetClaimQuery>
{
    public GetClaimQueryValidator()
    {
        RuleFor(query => query.ClaimId).NotEmpty();
    }
}

public sealed class GetClaimQueryHandler(
    IClaimReader claims,
    IClaimReserveReader reserves,
    IClaimDocumentReader documents,
    IAuditLogReader audit,
    IStorageService storage) : IRequestHandler<GetClaimQuery, Result<ClaimDetailDto>>
{
    private const int RecentAuditCount = 10;

    public async Task<Result<ClaimDetailDto>> Handle(GetClaimQuery query, CancellationToken cancellationToken)
    {
        var loaded = await claims.GetDetailAsync(query.ClaimId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded;
        }

        var files = await documents.ListAsync(query.ClaimId, cancellationToken);
        var recent = await audit.ListAsync(query.ClaimId, new PageWindow(1, RecentAuditCount), cancellationToken);
        var detail = loaded.Value with
        {
            Reserves = await reserves.GetReservesAsync(query.ClaimId, cancellationToken),
            Documents = await ClaimDocumentLinks.BuildAsync(storage, files, cancellationToken),
            RecentAudit = recent.Items,
        };

        return Result<ClaimDetailDto>.Success(detail);
    }
}
