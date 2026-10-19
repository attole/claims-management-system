using ClaimsModule.Domain.ReserveComponents;

namespace ClaimsModule.Application.ReserveComponents;

public interface IReserveComponentRepository
{
    Task<IReadOnlyList<ReserveComponent>> ListByClaimAsync(Guid claimId, CancellationToken cancellationToken);
}
