namespace ClaimsModule.Domain.ReserveComponents.Flows;

internal sealed class ReopenReserveComponentTransition
    : ReserveComponentBaseTransition<ReopenReserveComponentTransition>
{
    public ReopenReserveComponentTransition() : base(new ReserveComponentActiveState())
    {
        AllowedFrom(new ReserveComponentClosedState());
    }
}
