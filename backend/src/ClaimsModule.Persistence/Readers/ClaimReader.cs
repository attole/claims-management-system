using AutoMapper;
using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Claims.Issues;
using ClaimsModule.Application.Claims.Loss;
using ClaimsModule.Application.Claims.Parties;
using ClaimsModule.Application.Claims.RiskObjects;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Persistence.Rows;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Readers;

internal sealed class ClaimReader(ClaimsDbContext context, IMapper mapper) : IClaimReader
{
    public async Task<bool> ExistsAsync(Guid claimId, CancellationToken cancellationToken)
    {
        bool exists = await context.Claims.AnyAsync(claim => claim.Id == claimId, cancellationToken);

        return exists;
    }

    public async Task<PagedResult<ClaimSummaryDto>> ListAsync(
        ClaimListFilter filter,
        PageWindow page,
        CancellationToken cancellationToken)
    {
        var rows = context.Set<ClaimListRow>().AsNoTracking();
        var matches = FilterByDates(FilterByOwner(FilterByText(rows, filter), filter), filter);
        int totalCount = await matches.CountAsync(cancellationToken);
        var pageRows = await matches
            .OrderByDescending(row => row.ClaimNumber)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);

        var totals = await LoadReserveTotalsAsync(pageRows, cancellationToken);
        var items = pageRows.Select(row => ToSummary(row, totals)).ToList();

        return new PagedResult<ClaimSummaryDto>(items, page.Page, page.PageSize, totalCount);
    }

    public async Task<Result<ClaimDetailDto>> GetDetailAsync(Guid claimId, CancellationToken cancellationToken)
    {
        var claim = await context.Claims
            .AsNoTracking()
            .Include(found => found.LossEvent)
            .Include(found => found.Parties)
            .Include(found => found.RiskObjects)
            .Include(found => found.Issues)
            .AsSplitQuery()
            .FirstOrDefaultAsync(found => found.Id == claimId, cancellationToken);

        if (claim is null)
        {
            return Result<ClaimDetailDto>.Failure([ClaimErrors.ClaimNotFound(claimId)]);
        }

        var policyNames = await LoadPolicyNamesAsync(claim.PolicyId, cancellationToken);
        var detail = ToDetail(claim, policyNames);

        return Result<ClaimDetailDto>.Success(detail);
    }

    private static IQueryable<ClaimListRow> FilterByText(IQueryable<ClaimListRow> rows, ClaimListFilter filter)
    {
        var statusNames = filter.Statuses.Select(kind => ClaimState.From(kind).Name).ToList();
        string search = filter.Search;

        if (statusNames.Count > 0)
        {
            rows = rows.Where(row => statusNames.Contains(row.Status));
        }

        if (search.Length > 0)
        {
            rows = rows.Where(row => row.ClaimNumber.Contains(search) || row.ClientName.Contains(search));
        }

        return rows;
    }

    private static IQueryable<ClaimListRow> FilterByOwner(IQueryable<ClaimListRow> rows, ClaimListFilter filter)
    {
        var handlerId = filter.AssignedHandlerId;
        var policyId = filter.PolicyId;
        string causeCode = filter.CauseOfLossCode;

        if (handlerId != Guid.Empty)
        {
            rows = rows.Where(row => row.AssignedHandlerId == handlerId);
        }

        if (policyId != Guid.Empty)
        {
            rows = rows.Where(row => row.PolicyId == policyId);
        }

        if (causeCode.Length > 0)
        {
            rows = rows.Where(row => row.CauseOfLossCode == causeCode);
        }

        return rows;
    }

    private static IQueryable<ClaimListRow> FilterByDates(IQueryable<ClaimListRow> rows, ClaimListFilter filter)
    {
        if (filter.DateFrom != DateOnly.MinValue)
        {
            var start = StartOfDay(filter.DateFrom);
            rows = rows.Where(row => row.LossDate >= start);
        }

        if (filter.DateTo != DateOnly.MaxValue)
        {
            var before = StartOfDay(filter.DateTo.AddDays(1));
            rows = rows.Where(row => row.LossDate < before);
        }

        return rows;
    }

    private static DateTimeOffset StartOfDay(DateOnly date)
    {
        return new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    }

    private static ClaimSummaryDto ToSummary(ClaimListRow row, IReadOnlyDictionary<Guid, decimal> totals)
    {
        var summary = new ClaimSummaryDto
        {
            Id = row.ClaimId,
            ClaimNumber = row.ClaimNumber,
            ClientName = row.ClientName,
            PolicyNumber = row.PolicyNumber,
            LossDate = row.LossDate ?? DateTimeOffset.MinValue,
            Status = row.Status,
            TotalReserves = totals.GetValueOrDefault(row.ClaimId),
        };

        return summary;
    }

    private async Task<Dictionary<Guid, decimal>> LoadReserveTotalsAsync(
        IReadOnlyList<ClaimListRow> rows,
        CancellationToken cancellationToken)
    {
        var claimIds = rows.Select(row => row.ClaimId).ToList();
        var components = await context.ReserveComponents
            .AsNoTracking()
            .Include(component => component.History)
            .Where(component => claimIds.Contains(component.ClaimId))
            .ToListAsync(cancellationToken);

        var totals = components
            .GroupBy(component => component.ClaimId)
            .ToDictionary(group => group.Key, group => group.Sum(component => component.ApprovedBalance));

        return totals;
    }

    private async Task<PolicyNames> LoadPolicyNamesAsync(Guid policyId, CancellationToken cancellationToken)
    {
        var names = await context.Policies
            .AsNoTracking()
            .Where(policy => policy.Id == policyId)
            .Select(policy => new PolicyNames(policy.PolicyNumber, policy.ClientName))
            .ToListAsync(cancellationToken);

        return names.FirstOrDefault(PolicyNames.Empty);
    }

    private ClaimDetailDto ToDetail(Claim claim, PolicyNames policyNames)
    {
        var detail = new ClaimDetailDto
        {
            Claim = mapper.Map<ClaimDto>(claim),
            ClientName = policyNames.ClientName,
            PolicyNumber = policyNames.PolicyNumber,
            Loss = mapper.Map<LossDetailsDto>(claim.LossEvent),
            Parties = claim.Parties.Select(party => mapper.Map<PartyDto>(party)).ToList(),
            RiskObjects = claim.RiskObjects.Select(riskObject => mapper.Map<RiskObjectDto>(riskObject)).ToList(),
            Issues = claim.Issues
                .Where(issue => issue.State.IsCurrent)
                .Select(issue => mapper.Map<IssueDto>(issue))
                .ToList(),
        };

        return detail;
    }

    private sealed record PolicyNames(string PolicyNumber, string ClientName)
    {
        public static PolicyNames Empty { get; } = new(string.Empty, string.Empty);
    }
}
