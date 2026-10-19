using ClaimsModule.Application.Paging;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Application.Policies;

public interface IPolicyReader
{
    Task<bool> ExistsAsync(Guid policyId, CancellationToken cancellationToken);

    Task<PolicyPeriod> GetPeriodAsync(Guid policyId, CancellationToken cancellationToken);

    Task<PagedResult<PolicyDto>> SearchAsync(string text, PageWindow page, CancellationToken cancellationToken);
}
