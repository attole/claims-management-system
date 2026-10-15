using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Transitions;

namespace ClaimsModule.Domain.Claims.Root.Bases;

internal abstract class ClaimBaseTransition<TSelf, TInput>(ClaimState target)
    : Transition<TSelf, Claim, ClaimState, TInput>(target)
    where TSelf : ClaimBaseTransition<TSelf, TInput>, new()
    where TInput : ClaimBaseTransitionInput
{
    protected sealed override StateChange<ClaimState> Apply(Claim claim, TInput input)
    {
        var change = new ClaimStateChange(
            Ids.Next(input.Now),
            claim.Id,
            claim.OrganisationId,
            claim.State,
            Target,
            input.Reason,
            input.Note,
            input.Now);
        claim.StateChangeList.Add(change);
        claim.State = Target;

        return new StateChange<ClaimState>(change.From, change.To);
    }
}
