using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Dtos;
using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.Results;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Audit.Queries;

public sealed record ListClaimAuditQuery(Guid ClaimId, int Page, int PageSize)
    : IQuery<Result<PagedResult<AuditEntryDto>>>, IPagedQuery;

public sealed class ListClaimAuditQueryValidator : PagedQueryValidator<ListClaimAuditQuery>
{
    public ListClaimAuditQueryValidator()
    {
        RuleFor(query => query.ClaimId).NotEmpty();
    }
}

public sealed class ListClaimAuditQueryHandler(IClaimReader claims, IAuditLogReader audit)
    : IRequestHandler<ListClaimAuditQuery, Result<PagedResult<AuditEntryDto>>>
{
    public async Task<Result<PagedResult<AuditEntryDto>>> Handle(
        ListClaimAuditQuery query,
        CancellationToken cancellationToken)
    {
        bool isKnown = await claims.ExistsAsync(query.ClaimId, cancellationToken);

        if (!isKnown)
        {
            return Result<PagedResult<AuditEntryDto>>.Failure([ClaimErrors.ClaimNotFound(query.ClaimId)]);
        }

        var entries = await audit.ListAsync(query.ClaimId, PageWindow.From(query), cancellationToken);

        return Result<PagedResult<AuditEntryDto>>.Success(entries);
    }
}
