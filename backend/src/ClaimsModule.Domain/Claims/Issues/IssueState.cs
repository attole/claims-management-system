using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.Claims.Issues;

public enum IssueStateKind
{
    Raised,
    Cleared,
    Acknowledged,
    Waived,
}

public abstract record IssueState(IssueStateKind Marker) : Marked<IssueStateKind>(Marker)
{
    public abstract bool IsResolved { get; }

    public virtual bool IsCurrent => true;

    public virtual bool IsWaived => false;
}

public sealed record IssueRaisedState() : IssueState(IssueStateKind.Raised)
{
    public override bool IsResolved => false;
}

public sealed record IssueClearedState() : IssueState(IssueStateKind.Cleared)
{
    public override bool IsResolved => true;

    public override bool IsCurrent => false;
}

public sealed record IssueAcknowledgedState() : IssueState(IssueStateKind.Acknowledged)
{
    public override bool IsResolved => true;
}

public sealed record IssueWaivedState() : IssueState(IssueStateKind.Waived)
{
    public override bool IsResolved => true;

    public override bool IsWaived => true;
}
