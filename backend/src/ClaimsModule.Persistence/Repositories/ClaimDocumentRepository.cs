using ClaimsModule.Application.ClaimDocuments;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.ClaimDocuments;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Repositories;

internal sealed class ClaimDocumentRepository(ClaimsDbContext context) : IClaimDocumentRepository
{
    public async Task<Result<ClaimDocument>> GetAsync(
        Guid claimId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var document = await context.ClaimDocuments
            .FirstOrDefaultAsync(found => found.ClaimId == claimId && found.Id == documentId, cancellationToken);

        if (document is null)
        {
            return Result<ClaimDocument>.Failure([DocumentErrors.NotFound(documentId)]);
        }

        return Result<ClaimDocument>.Success(document);
    }

    public async Task AddAsync(ClaimDocument document, CancellationToken cancellationToken)
    {
        await context.ClaimDocuments.AddAsync(document, cancellationToken);
    }
}
