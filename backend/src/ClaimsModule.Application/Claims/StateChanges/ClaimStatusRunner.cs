using System.Collections.Frozen;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Application.Claims.StateChanges;

public static class ClaimStatusRunner
{
    private static readonly FrozenDictionary<ClaimStateKind, Func<Claim, ClaimStatusContext, Outcome<Move<ClaimState>>>>
        _runners = new Dictionary<ClaimStateKind, Func<Claim, ClaimStatusContext, Outcome<Move<ClaimState>>>>
        {
            [ClaimStateKind.Open] = (claim, context) => claim.Open(context.ToTransitionInput()),
            [ClaimStateKind.UnderInvestigation] = (claim, context) => claim.Investigate(context.ToTransitionInput()),
            [ClaimStateKind.PendingPayment] = (claim, context) => claim.SendToPayment(context.ToSendToPaymentInput()),
            [ClaimStateKind.Closed] = (claim, context) => claim.Close(context.ToFreezeInput()),
            [ClaimStateKind.Withdrawn] = (claim, context) => claim.Withdraw(context.ToFreezeInput()),
            [ClaimStateKind.Reopened] = (claim, context) => claim.Reopen(context.ToReopenInput()),
        }.ToFrozenDictionary();

    public static Result<Move<ClaimState>> Run(Claim claim, ClaimStatusContext context)
    {
        var nextStatuses = NextStatusNames(claim);

        if (!_runners.TryGetValue(context.Command.TargetStatus, out var run))
        {
            var unsupported = ClaimErrors.StatusUnsupported(context.Command.TargetStatus.ToString(), nextStatuses);

            return Result<Move<ClaimState>>.Failure([unsupported]);
        }

        var result = run(claim, context).ToResult();
        var refusal = result.Errors.FirstOrDefault(error => error.Code == TransitionCodes.NotPermitted);

        if (refusal is null)
        {
            return result;
        }

        var explained = ClaimErrors.WithNextStatuses(refusal, nextStatuses);

        return Result<Move<ClaimState>>.Failure([explained]);
    }

    private static IReadOnlyList<string> NextStatusNames(Claim claim)
    {
        return ClaimTransitions.NextFrom(claim.State)
            .Select(state => state.Name)
            .ToList();
    }
}
