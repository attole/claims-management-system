using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.Claims;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Repositories;

internal sealed class ClaimRepository(ClaimsDbContext context) : IClaimRepository
{
    public async Task<Result<Claim>> GetAsync(Guid claimId, CancellationToken cancellationToken)
    {
        var claim = await context.Claims
            .Include(found => found.LossEvent)
            .Include(found => found.Parties)
            .Include(found => found.RiskObjects)
            .Include(found => found.Issues)
            .Include(found => found.StateChanges)
            .AsSplitQuery()
            .FirstOrDefaultAsync(found => found.Id == claimId, cancellationToken);

        if (claim is null)
        {
            return Result<Claim>.Failure([ClaimErrors.ClaimNotFound(claimId)]);
        }

        return Result<Claim>.Success(claim);
    }

    public async Task AddAsync(Claim claim, CancellationToken cancellationToken)
    {
        await context.Claims.AddAsync(claim, cancellationToken);
    }
}
