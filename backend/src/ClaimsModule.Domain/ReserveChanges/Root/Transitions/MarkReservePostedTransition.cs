using ClaimsModule.Domain.ReserveChanges.Root.Bases;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Transitions;

public sealed record MarkReservePostedInput(Guid ActorId, DateTimeOffset Now)
    : ReserveChangeBasePostingTransitionInput(ActorId, Now);

internal sealed class MarkReservePostedTransition : ReserveChangeBasePostingTransition<MarkReservePostedInput>
{
    public MarkReservePostedTransition() : base(new ReservePostingPostedState())
    {
        AllowedFrom(new ReservePostingPendingState(), new ReservePostingFailedState())
            .Require(new IsReserveChangeApproved());
    }
}
