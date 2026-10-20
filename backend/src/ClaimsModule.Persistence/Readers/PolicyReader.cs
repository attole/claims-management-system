using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.Policies;
using ClaimsModule.Domain.Policies;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Readers;

internal sealed class PolicyReader(ClaimsDbContext context, IClock clock) : IPolicyReader
{
    private const string EffectiveDate = "EffectiveDate";
    private const string ExpirationDate = "ExpirationDate";
    private const string CancelledDate = "CancelledDate";

    public async Task<bool> ExistsAsync(Guid policyId, CancellationToken cancellationToken)
    {
        bool exists = await context.Policies.AnyAsync(policy => policy.Id == policyId, cancellationToken);

        return exists;
    }

    public async Task<PolicyPeriod> GetPeriodAsync(Guid policyId, CancellationToken cancellationToken)
    {
        var rows = await ProjectRows(context.Policies.AsNoTracking().Where(policy => policy.Id == policyId))
            .ToListAsync(cancellationToken);

        var periods = rows.Select(row => ToPeriod(row)).ToList();

        return periods.FirstOrDefault(PolicyPeriod.Empty);
    }

    public async Task<PagedResult<PolicyDto>> SearchAsync(
        string text,
        PageWindow page,
        CancellationToken cancellationToken)
    {
        var matches = context.Policies
            .AsNoTracking()
            .Where(policy => policy.PolicyNumber.Contains(text) || policy.ClientName.Contains(text));
        int totalCount = await matches.CountAsync(cancellationToken);
        var pageMatches = matches.OrderBy(policy => policy.PolicyNumber).Skip(page.Skip).Take(page.PageSize);
        var rows = await ProjectRows(pageMatches).ToListAsync(cancellationToken);

        var coverageTypes = await LoadCoverageTypesAsync(rows, cancellationToken);
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var items = rows.Select(row => ToDto(row, coverageTypes, today)).ToList();

        return new PagedResult<PolicyDto>(items, page.Page, page.PageSize, totalCount);
    }

    private static IQueryable<PolicyRow> ProjectRows(IQueryable<Policy> policies)
    {
        var rows = policies.Select(policy => new PolicyRow(
            policy.Id,
            policy.PolicyNumber,
            policy.ClientName,
            EF.Property<DateOnly>(policy, EffectiveDate),
            EF.Property<DateOnly>(policy, ExpirationDate),
            EF.Property<DateOnly?>(policy, CancelledDate)));

        return rows;
    }

    private static PolicyPeriod ToPeriod(PolicyRow row)
    {
        return new PolicyPeriod(row.Id, row.EffectiveDate, row.ExpirationDate, row.CancelledDate ?? DateOnly.MinValue);
    }

    private async Task<ILookup<Guid, string>> LoadCoverageTypesAsync(
        IReadOnlyList<PolicyRow> rows,
        CancellationToken cancellationToken)
    {
        var policyIds = rows.Select(row => row.Id).ToList();
        var types = await context.PolicyCoverageTypes
            .AsNoTracking()
            .Where(type => policyIds.Contains(type.PolicyId))
            .OrderBy(type => type.Name)
            .ToListAsync(cancellationToken);

        return types.ToLookup(type => type.PolicyId, type => type.Name);
    }

    private static PolicyDto ToDto(PolicyRow row, ILookup<Guid, string> coverageTypes, DateOnly today)
    {
        var dto = new PolicyDto
        {
            Id = row.Id,
            PolicyNumber = row.PolicyNumber,
            ClientName = row.ClientName,
            EffectiveDate = row.EffectiveDate,
            ExpirationDate = row.ExpirationDate,
            Status = ToPeriod(row).StatusOn(today).ToString(),
            CoverageTypes = coverageTypes[row.Id].ToList(),
        };

        return dto;
    }

    private sealed record PolicyRow(
        Guid Id,
        string PolicyNumber,
        string ClientName,
        DateOnly EffectiveDate,
        DateOnly ExpirationDate,
        DateOnly? CancelledDate);
}
