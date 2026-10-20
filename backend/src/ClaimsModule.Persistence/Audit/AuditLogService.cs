using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Audit;

internal sealed class AuditLogService(ClaimsDbContext context) : IAuditLogService
{
    public async Task WriteAsync(AuditEntry entry, AuditContext auditContext, CancellationToken cancellationToken)
    {
        var organisationId = await FindOrganisationIdAsync(entry.ClaimId, cancellationToken);
        var row = new AuditLogEntry(Ids.Next(), organisationId, entry.ClaimId, entry.EventType, entry.Description)
        {
            OldValue = ToNullable(entry.OldValue),
            NewValue = ToNullable(entry.NewValue),
            RelatedEntityId = entry.RelatedEntityId == Guid.Empty ? null : entry.RelatedEntityId,
            RelatedEntityType = ToNullable(entry.RelatedEntityType),
            CorrelationId = auditContext.CorrelationId == Guid.Empty ? null : auditContext.CorrelationId,
        };

        context.AuditLog.Add(row);
    }

    private static string? ToNullable(string text)
    {
        return text.Length == 0 ? null : text;
    }

    private async Task<Guid> FindOrganisationIdAsync(Guid claimId, CancellationToken cancellationToken)
    {
        var tracked = context.ChangeTracker.Entries<Claim>()
            .Select(entry => entry.Entity)
            .FirstOrDefault(claim => claim.Id == claimId);

        if (tracked is not null)
        {
            return tracked.OrganisationId;
        }

        return await context.Claims
            .IgnoreQueryFilters()
            .Where(claim => claim.Id == claimId)
            .Select(claim => claim.OrganisationId)
            .SingleAsync(cancellationToken);
    }
}
