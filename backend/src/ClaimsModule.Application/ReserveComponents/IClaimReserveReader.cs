namespace ClaimsModule.Application.ReserveComponents;

public interface IClaimReserveReader
{
    Task<ClaimReserveFacts> GetFactsAsync(Guid claimId, CancellationToken cancellationToken);
}
