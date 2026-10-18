using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.ReserveComponents.Flows;

internal abstract class ReserveComponentBaseTransition<TSelf>(ReserveComponentState target)
    : Transition<TSelf, ReserveComponent, ReserveComponentState, NoInput>(target)
    where TSelf : ReserveComponentBaseTransition<TSelf>, new()
{
    protected sealed override void ApplyMove(
        ReserveComponent component,
        NoInput input,
        Move<ReserveComponentState> move)
    {
        component.State = move.To;
    }
}
