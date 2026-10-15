using ClaimsModule.Domain.ReserveChanges.Root.Bases;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Transitions;

public sealed record CancelReservePostingInput(Guid ActorId, DateTimeOffset Now)
    : ReserveChangeBasePostingTransitionInput(ActorId, Now);

internal sealed class CancelReservePostingTransition : ReserveChangeBasePostingTransition<CancelReservePostingInput>
{
    public CancelReservePostingTransition() : base(new ReservePostingCancelledState())
    {
        AllowedFrom(new ReservePostingPendingState())
            .Require(new IsReserveChangeRefused());
    }
}
