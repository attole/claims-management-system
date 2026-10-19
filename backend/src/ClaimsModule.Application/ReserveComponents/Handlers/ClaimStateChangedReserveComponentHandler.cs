using ClaimsModule.Application.Events;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Events;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents;

namespace ClaimsModule.Application.ReserveComponents.Handlers;

public sealed class ClaimStateChangedReserveComponentHandler(IReserveComponentRepository components)
    : IBeforeCommitEventHandler<ClaimStateChangedEvent>
{
    private const string RefusedMessage = "A reserve component refused to follow its claim:";

    public async Task HandleAsync(
        ClaimStateChangedEvent domainEvent,
        EventContext context,
        CancellationToken cancellationToken)
    {
        var target = ClaimState.From(Enum.Parse<ClaimStateKind>(domainEvent.To));
        var loaded = await components.ListByClaimAsync(domainEvent.ClaimId, cancellationToken);

        bool isReopened = target.Marker == ClaimStateKind.Reopened;
        var affected = loaded
            .Where(_ => isReopened || !target.IsEditable)
            .Where(component => component.State.IsActive != isReopened);

        foreach (var component in affected)
        {
            var outcome = isReopened ? component.Reopen() : component.Close();

            RequireSuccess(outcome);
        }
    }

    private static void RequireSuccess(Outcome<Move<ReserveComponentState>> outcome)
    {
        if (!outcome.IsSuccess)
        {
            var codes = string.Join(", ", outcome.CriticalFindings.Select(finding => finding.Code));

            throw new InvalidOperationException($"{RefusedMessage} {codes}.");
        }
    }
}
