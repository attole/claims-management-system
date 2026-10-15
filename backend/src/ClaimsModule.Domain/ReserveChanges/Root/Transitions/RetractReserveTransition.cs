using ClaimsModule.Domain.Common.Roles;
using ClaimsModule.Domain.ReserveChanges.Root.Bases;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Transitions;

public sealed record RetractReserveInput(UserRole ActorRole, Guid ActorId, DateTimeOffset Now, ReserveSnapshot Snapshot)
    : ReserveChangeBaseTransitionInput(ActorRole, ActorId, Now, Snapshot);

public sealed class RetractReserveTransition : ReserveChangeBaseTransition<RetractReserveInput>
{
    public RetractReserveTransition()
        : base(new ReserveCancelledState())
    {
        AllowedFrom(new ReservePendingApprovalState())
            .Require(input => new IsSubmitter(input.ActorId));
    }
}
