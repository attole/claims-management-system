using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Application.Claims.StateChanges;

public sealed record ClaimStatusContext(
    ChangeClaimStatusCommand Command,
    PolicyPeriod PolicyPeriod,
    CauseCodes CauseCodes,
    ClaimReserveFacts ReserveFacts,
    UserRole ActorRole)
{
    public ClaimTransitionInput ToTransitionInput()
    {
        return new ClaimTransitionInput(PolicyPeriod, CauseCodes)
        {
            Reason = ToReason(),
            Note = Command.Note ?? string.Empty,
        };
    }

    public FreezeClaimInput ToFreezeInput()
    {
        return new FreezeClaimInput(
            ReserveFacts.HasPendingHistory,
            ReserveFacts.HasUnpostedHistory,
            ReserveFacts.HasOpenBalance,
            PolicyPeriod,
            CauseCodes)
        {
            Reason = ToReason(),
            Note = Command.Note ?? string.Empty,
        };
    }

    public SendToPaymentClaimInput ToSendToPaymentInput()
    {
        return new SendToPaymentClaimInput(ReserveFacts.HasApprovedReserve, PolicyPeriod, CauseCodes)
        {
            IsLiabilityConfirmed = Command.IsLiabilityConfirmed,
            Reason = ToReason(),
            Note = Command.Note ?? string.Empty,
        };
    }

    public ReopenClaimInput ToReopenInput()
    {
        return new ReopenClaimInput(ActorRole, PolicyPeriod, CauseCodes)
        {
            Reason = ToReason(),
            Note = Command.Note ?? string.Empty,
        };
    }

    private StateChangeReason ToReason()
    {
        return new StateChangeReason(Command.ReasonCode ?? string.Empty, Command.ReasonText ?? string.Empty);
    }
}
