using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Domain.CauseOfLoss;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Readers;

internal sealed class CauseCodeReader(ClaimsDbContext context, IMapper mapper) : ICauseCodeReader
{
    private const string SortOrder = "SortOrder";

    public async Task<CauseCodes> GetActiveAsync(CancellationToken cancellationToken)
    {
        var codes = await context.CauseOfLossCodes
            .AsNoTracking()
            .Where(code => code.IsActive)
            .Select(code => code.Code)
            .ToListAsync(cancellationToken);

        return new CauseCodes(codes);
    }

    public async Task<IReadOnlyList<CauseCodeDto>> ListActiveAsync(
        IReadOnlyList<PerilCategory> perils,
        CancellationToken cancellationToken)
    {
        var active = context.CauseOfLossCodes.AsNoTracking().Where(code => code.IsActive);
        var matches = perils.Count > 0 ? active.Where(code => perils.Contains(code.PerilCategory)) : active;
        var dtos = await matches
            .OrderBy(code => EF.Property<int>(code, SortOrder))
            .ThenBy(code => code.Name)
            .ProjectTo<CauseCodeDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return dtos;
    }
}
