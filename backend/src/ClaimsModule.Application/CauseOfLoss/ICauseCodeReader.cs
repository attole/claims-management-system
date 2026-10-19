using ClaimsModule.Domain.CauseOfLoss;

namespace ClaimsModule.Application.CauseOfLoss;

public interface ICauseCodeReader
{
    Task<CauseCodes> GetActiveAsync(CancellationToken cancellationToken);
}
