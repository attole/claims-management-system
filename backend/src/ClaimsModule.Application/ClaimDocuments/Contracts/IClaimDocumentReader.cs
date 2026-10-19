namespace ClaimsModule.Application.ClaimDocuments.Contracts;

public interface IClaimDocumentReader
{
    Task<IReadOnlyList<ClaimDocumentFile>> ListAsync(Guid claimId, CancellationToken cancellationToken);
}
