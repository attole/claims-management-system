using ClaimsModule.Application.Results;
using ClaimsModule.Domain.ClaimDocuments;

namespace ClaimsModule.Application.ClaimDocuments.Contracts;

public interface IClaimDocumentRepository
{
    Task<Result<ClaimDocument>> GetAsync(Guid claimId, Guid documentId, CancellationToken cancellationToken);

    Task AddAsync(ClaimDocument document, CancellationToken cancellationToken);
}
