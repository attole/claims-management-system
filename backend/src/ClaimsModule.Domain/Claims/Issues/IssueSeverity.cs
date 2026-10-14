using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.Claims.Issues;

public enum IssueSeverityKind
{
    Critical,
    Warning,
}

public abstract record IssueSeverity(IssueSeverityKind Marker) : Marked<IssueSeverityKind>(Marker)
{
    public abstract bool IsBlockingOpening { get; }
}

public sealed record IssueCriticalSeverity() : IssueSeverity(IssueSeverityKind.Critical)
{
    public override bool IsBlockingOpening => true;
}

public sealed record IssueWarningSeverity() : IssueSeverity(IssueSeverityKind.Warning)
{
    public override bool IsBlockingOpening => false;
}
