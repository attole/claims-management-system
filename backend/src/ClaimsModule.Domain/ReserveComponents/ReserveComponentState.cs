using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.ReserveComponents;

public enum ReserveComponentStateKind
{
    Active,
    Closed,
}

public abstract record ReserveComponentState(ReserveComponentStateKind Marker)
    : Marked<ReserveComponentStateKind>(Marker)
{
    public virtual bool IsActive => false;
}

public sealed record ReserveComponentActiveState() : ReserveComponentState(ReserveComponentStateKind.Active)
{
    public override bool IsActive => true;
}

public sealed record ReserveComponentClosedState() : ReserveComponentState(ReserveComponentStateKind.Closed);
