using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Dtos;
using ClaimsModule.Application.Paging;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Audit;

internal sealed class AuditLogReader(ClaimsDbContext context) : IAuditLogReader
{
    public async Task<PagedResult<AuditEntryDto>> ListAsync(
        Guid claimId,
        PageWindow page,
        CancellationToken cancellationToken)
    {
        var entries = context.AuditLog.AsNoTracking().Where(entry => entry.ClaimId == claimId);
        var totalCount = await entries.CountAsync(cancellationToken);
        var rows = await entries
            .OrderByDescending(entry => entry.CreatedAt)
            .ThenByDescending(entry => entry.Id)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);

        var items = rows.Select(row => ToDto(row)).ToList();

        return new PagedResult<AuditEntryDto>(items, page.Page, page.PageSize, totalCount);
    }

    private static AuditEntryDto ToDto(AuditLogEntry row)
    {
        return new AuditEntryDto
        {
            Id = row.Id,
            ClaimId = row.ClaimId,
            EventType = row.EventType,
            Description = row.Description,
            OldValue = row.OldValue ?? string.Empty,
            NewValue = row.NewValue ?? string.Empty,
            RelatedEntityId = row.RelatedEntityId ?? Guid.Empty,
            RelatedEntityType = row.RelatedEntityType ?? string.Empty,
            CorrelationId = row.CorrelationId ?? Guid.Empty,
            CreatedAt = row.CreatedAt,
            CreatedByUserId = row.UserCreated ?? Guid.Empty,
        };
    }
}
