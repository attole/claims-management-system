using AutoMapper;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.ReserveComponents.Dtos;
using ClaimsModule.Application.ReserveComponents.History.Dtos;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.History;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Readers;

internal sealed class ClaimReserveReader(ClaimsDbContext context, IMapper mapper) : IClaimReserveReader
{
    private static readonly ReserveApprovalState[] _approvedStates =
    [
        new ReserveApprovedState(),
        new ReserveAutoApprovedState(),
    ];

    private static readonly ReservePostingState _pendingPosting = new ReservePostingPendingState();

    public async Task<ClaimReserveFacts> GetFactsAsync(Guid claimId, CancellationToken cancellationToken)
    {
        var components = await LoadComponentsAsync(claimId, cancellationToken);
        var history = components.SelectMany(component => component.History).ToList();

        var facts = new ClaimReserveFacts(
            history.Any(row => row.ApprovalState.Marker == ReserveApprovalStateKind.PendingApproval),
            history.Any(row => row.ApprovalState.IsApproved && IsUnposted(row.PostingState)),
            components.Any(component => component.ApprovedBalance > 0m),
            history.Any(row => row.ApprovalState.IsApproved),
            components.Sum(component => component.ApprovedBalance));

        return facts;
    }

    public async Task<ClaimReservesDto> GetReservesAsync(Guid claimId, CancellationToken cancellationToken)
    {
        var components = await LoadComponentsAsync(claimId, cancellationToken);

        var componentDtos = components
            .Select(component => mapper.Map<ReserveComponentDto>(component))
            .ToList();
        var historyDtos = components
            .SelectMany(component => component.History)
            .OrderBy(history => history.SubmittedAt)
            .ThenBy(history => history.Details.ChangeSequence)
            .Select(history => mapper.Map<ReserveHistoryDto>(history))
            .ToList();

        var reserves = new ClaimReservesDto
        {
            ApprovedTotal = components.Sum(component => component.ApprovedBalance),
            Components = componentDtos,
            History = historyDtos,
        };

        return reserves;
    }

    public async Task<IReadOnlyList<UnpostedReserve>> ListStalePendingPostingsAsync(
        DateTimeOffset approvedBefore,
        CancellationToken cancellationToken)
    {
        var approvedStates = _approvedStates;
        var pendingPosting = _pendingPosting;
        var unposted = await context.Set<ReserveHistory>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(history => approvedStates.Contains(history.ApprovalState))
            .Where(history => history.PostingState == pendingPosting)
            .Select(history => new { history.Details.ClaimId, history.Id, history.Review.ReviewedAt })
            .ToListAsync(cancellationToken);

        var stale = unposted
            .Where(row => row.ReviewedAt < approvedBefore)
            .Select(row => new UnpostedReserve(row.ClaimId, row.Id))
            .ToList();

        return stale;
    }

    private static bool IsUnposted(ReservePostingState state)
    {
        bool isUnposted = state.Marker is ReservePostingStateKind.Pending or ReservePostingStateKind.Failed;

        return isUnposted;
    }

    private async Task<List<ReserveComponent>> LoadComponentsAsync(Guid claimId, CancellationToken cancellationToken)
    {
        var components = await context.ReserveComponents
            .AsNoTracking()
            .Include(component => component.History)
            .Where(component => component.ClaimId == claimId)
            .ToListAsync(cancellationToken);

        return components;
    }
}
