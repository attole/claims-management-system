using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.ReserveComponents;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Repositories;

internal sealed class ReserveComponentRepository(ClaimsDbContext context) : IReserveComponentRepository
{
    public async Task<IReadOnlyList<ReserveComponent>> ListByClaimAsync(
        Guid claimId,
        CancellationToken cancellationToken)
    {
        var components = await context.ReserveComponents
            .Include(component => component.History)
            .Where(component => component.ClaimId == claimId)
            .ToListAsync(cancellationToken);

        return components;
    }

    public async Task<Result<ReserveComponent>> GetAsync(
        Guid claimId,
        Guid componentId,
        CancellationToken cancellationToken)
    {
        var component = await context.ReserveComponents
            .Include(found => found.History)
            .FirstOrDefaultAsync(found => found.ClaimId == claimId && found.Id == componentId, cancellationToken);

        if (component is null)
        {
            return Result<ReserveComponent>.Failure([ReserveErrors.ComponentNotFound(componentId)]);
        }

        return Result<ReserveComponent>.Success(component);
    }

    public async Task<Result<ReserveComponent>> GetByHistoryAsync(
        Guid claimId,
        Guid reserveHistoryId,
        CancellationToken cancellationToken)
    {
        var component = await context.ReserveComponents
            .Include(found => found.History)
            .Where(found => found.ClaimId == claimId)
            .FirstOrDefaultAsync(
                found => found.History.Any(history => history.Id == reserveHistoryId),
                cancellationToken);

        if (component is null)
        {
            return Result<ReserveComponent>.Failure([ReserveErrors.HistoryNotFound(reserveHistoryId)]);
        }

        return Result<ReserveComponent>.Success(component);
    }

    public async Task AddAsync(ReserveComponent component, CancellationToken cancellationToken)
    {
        await context.ReserveComponents.AddAsync(component, cancellationToken);
    }
}
