using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Sla;
using ClaimsModule.Domain.Claims;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Readers;

internal sealed class SlaClaimReader(ClaimsDbContext context) : ISlaClaimReader
{
    private static readonly ClaimState[] _watchedStates = [new ClaimDraftState(), new ClaimOpenState()];

    public async Task<IReadOnlyList<Guid>> ListBreachedClaimIdsAsync(
        DateTimeOffset notUpdatedSince,
        DateTimeOffset notBreachedSince,
        CancellationToken cancellationToken)
    {
        var watchedStates = _watchedStates;
        var recentBreaches = context.AuditLog
            .IgnoreQueryFilters()
            .Where(entry => entry.EventType == AuditEventTypes.SlaBreachDetected)
            .Where(entry => EF.Property<DateTimeOffset>(entry, RowColumns.CreatedAt) >= notBreachedSince);

        var breached = await context.Claims
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(claim => !EF.Property<bool>(claim, RowColumns.IsDeleted))
            .Where(claim => watchedStates.Contains(claim.State))
            .Where(claim => (EF.Property<DateTimeOffset?>(claim, RowColumns.UpdatedAt)
                ?? EF.Property<DateTimeOffset>(claim, RowColumns.CreatedAt)) < notUpdatedSince)
            .Where(claim => !recentBreaches.Any(entry => entry.ClaimId == claim.Id))
            .Select(claim => claim.Id)
            .ToListAsync(cancellationToken);

        return breached;
    }
}
