using ClaimsModule.Domain.Common.Roles;
using ClaimsModule.Domain.ReserveChanges.Root.Bases;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Transitions;

public sealed record RejectReserveInput(UserRole ActorRole, Guid ActorId, DateTimeOffset Now, ReserveSnapshot Snapshot)
    : ReserveChangeBaseTransitionInput(ActorRole, ActorId, Now, Snapshot);

public sealed class RejectReserveTransition : ReserveChangeBaseTransition<RejectReserveInput>
{
    public RejectReserveTransition()
        : base(new ReserveRejectedState())
    {
        AllowedFrom(new ReservePendingApprovalState())
            .Require(input => new HasApprovalRole(input.ActorRole))
            .Require(input => new HasRejectionReason(input.Reason));
    }
}
