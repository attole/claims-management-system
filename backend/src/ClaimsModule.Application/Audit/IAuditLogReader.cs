using ClaimsModule.Application.Paging;

namespace ClaimsModule.Application.Audit;

public interface IAuditLogReader
{
    Task<PagedResult<AuditEntryDto>> ListAsync(Guid claimId, PageWindow page, CancellationToken cancellationToken);
}
