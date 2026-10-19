using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.Results;

namespace ClaimsModule.Application.Claims.Contracts;

public interface IClaimReader
{
    Task<bool> ExistsAsync(Guid claimId, CancellationToken cancellationToken);

    Task<PagedResult<ClaimSummaryDto>> ListAsync(
        ClaimListFilter filter,
        PageWindow page,
        CancellationToken cancellationToken);

    Task<Result<ClaimDetailDto>> GetDetailAsync(Guid claimId, CancellationToken cancellationToken);
}
