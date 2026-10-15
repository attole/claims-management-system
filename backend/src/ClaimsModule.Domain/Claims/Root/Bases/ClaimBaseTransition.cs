using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Transitions;

namespace ClaimsModule.Domain.Claims.Root.Bases;

internal abstract class ClaimBaseTransition<TInput>(ClaimState target) : Transition<Claim, ClaimState, TInput>(target)
    where TInput : ClaimBaseTransitionInput
{
    protected sealed override ClaimState Apply(Claim claim, TInput input)
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

        return claim.State;
    }
}
