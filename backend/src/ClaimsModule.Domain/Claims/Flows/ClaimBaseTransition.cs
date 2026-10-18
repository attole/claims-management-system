using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Flows;

internal abstract class ClaimBaseTransition<TSelf, TInput>(ClaimState target)
    : Transition<TSelf, Claim, ClaimState, TInput>(target)
    where TSelf : ClaimBaseTransition<TSelf, TInput>, new()
    where TInput : ClaimTransitionInput
{
    protected sealed override void ApplyMove(Claim claim, TInput input, Move<ClaimState> move)
    {
        var history = new ClaimStateChange(
            Ids.Next(),
            claim.Id,
            claim.OrganisationId,
            move.From,
            move.To,
            input.Reason,
            input.Note);
        claim.AttachStateChange(history);
        claim.State = move.To;
    }
}
