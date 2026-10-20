using ClaimsModule.Application.ReserveComponents.Dtos;

namespace ClaimsModule.Application.ReserveComponents.Contracts;

public interface IClaimReserveReader
{
    Task<ClaimReserveFacts> GetFactsAsync(Guid claimId, CancellationToken cancellationToken);

    Task<ClaimReservesDto> GetReservesAsync(Guid claimId, CancellationToken cancellationToken);

    Task<IReadOnlyList<UnpostedReserve>> ListStalePendingPostingsAsync(
        DateTimeOffset approvedBefore,
        CancellationToken cancellationToken);
}
