using ClaimsModule.Domain.ReserveChanges.Root.Bases;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Transitions;

public sealed record MarkReservePostingFailedInput(Guid ActorId, DateTimeOffset Now)
    : ReserveChangeBasePostingTransitionInput(ActorId, Now);

internal sealed class MarkReservePostingFailedTransition
    : ReserveChangeBasePostingTransition<MarkReservePostingFailedInput>
{
    public MarkReservePostingFailedTransition() : base(new ReservePostingFailedState())
    {
        AllowedFrom(new ReservePostingPendingState())
            .Require(new IsReserveChangeApproved());
    }
}
