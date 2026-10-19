using ClaimsModule.Application.Audit.Dtos;
using ClaimsModule.Application.Paging;

namespace ClaimsModule.Application.Audit.Contracts;

public interface IAuditLogReader
{
    Task<PagedResult<AuditEntryDto>> ListAsync(Guid claimId, PageWindow page, CancellationToken cancellationToken);
}
