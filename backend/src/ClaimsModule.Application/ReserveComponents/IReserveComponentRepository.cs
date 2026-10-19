using ClaimsModule.Application.Results;
using ClaimsModule.Domain.ReserveComponents;

namespace ClaimsModule.Application.ReserveComponents;

public interface IReserveComponentRepository
{
    Task<IReadOnlyList<ReserveComponent>> ListByClaimAsync(Guid claimId, CancellationToken cancellationToken);

    Task<Result<ReserveComponent>> GetAsync(Guid claimId, Guid componentId, CancellationToken cancellationToken);

    Task<Result<ReserveComponent>> GetByHistoryAsync(
        Guid claimId,
        Guid reserveHistoryId,
        CancellationToken cancellationToken);

    Task AddAsync(ReserveComponent component, CancellationToken cancellationToken);
}
