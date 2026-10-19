using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.ClaimDocuments.Dtos;
using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Results;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.ClaimDocuments.Queries;

public sealed record ListClaimDocumentsQuery(Guid ClaimId) : IQuery<Result<IReadOnlyList<ClaimDocumentLinkDto>>>;

public sealed class ListClaimDocumentsQueryValidator : AbstractValidator<ListClaimDocumentsQuery>
{
    public ListClaimDocumentsQueryValidator()
    {
        RuleFor(query => query.ClaimId).NotEmpty();
    }
}

public sealed class ListClaimDocumentsQueryHandler(
    IClaimReader claims,
    IClaimDocumentReader documents,
    IStorageService storage) : IRequestHandler<ListClaimDocumentsQuery, Result<IReadOnlyList<ClaimDocumentLinkDto>>>
{
    public async Task<Result<IReadOnlyList<ClaimDocumentLinkDto>>> Handle(
        ListClaimDocumentsQuery query,
        CancellationToken cancellationToken)
    {
        bool isKnown = await claims.ExistsAsync(query.ClaimId, cancellationToken);

        if (!isKnown)
        {
            return Result<IReadOnlyList<ClaimDocumentLinkDto>>.Failure([ClaimErrors.ClaimNotFound(query.ClaimId)]);
        }

        var files = await documents.ListAsync(query.ClaimId, cancellationToken);
        var links = await ClaimDocumentLinks.BuildAsync(storage, files, cancellationToken);

        return Result<IReadOnlyList<ClaimDocumentLinkDto>>.Success(links);
    }
}
