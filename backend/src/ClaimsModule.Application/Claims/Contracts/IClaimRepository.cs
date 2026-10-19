using ClaimsModule.Application.Results;
using ClaimsModule.Domain.Claims;

namespace ClaimsModule.Application.Claims.Contracts;

public interface IClaimRepository
{
    Task<Result<Claim>> GetAsync(Guid claimId, CancellationToken cancellationToken);

    Task AddAsync(Claim claim, CancellationToken cancellationToken);
}
