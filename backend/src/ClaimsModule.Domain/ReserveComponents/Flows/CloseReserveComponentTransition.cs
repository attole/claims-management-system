namespace ClaimsModule.Domain.ReserveComponents.Flows;

internal sealed class CloseReserveComponentTransition
    : ReserveComponentBaseTransition<CloseReserveComponentTransition>
{
    public CloseReserveComponentTransition() : base(new ReserveComponentClosedState())
    {
        AllowedFrom(new ReserveComponentActiveState());
    }
}
