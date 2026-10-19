using ClaimsModule.Domain.CauseOfLoss;

namespace ClaimsModule.Application.CauseOfLoss;

public interface ICauseCodeReader
{
    Task<CauseCodes> GetActiveAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<CauseCodeDto>> ListActiveAsync(
        IReadOnlyList<PerilCategory> perils,
        CancellationToken cancellationToken);
}
