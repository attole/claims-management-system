using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Issues;

public sealed record IssueFinding(string Code, string Message, string Field, IssueSeverity Severity) : IFinding
{
    public static IssueFinding Empty { get; } =
        new(string.Empty, string.Empty, string.Empty, new IssueWarningSeverity());

    public bool IsBlocking => Severity.IsBlockingOpening;

    public static IssueFinding Critical(string code, string message, string field)
    {
        return new IssueFinding(code, message, field, new IssueCriticalSeverity());
    }

    public static IssueFinding Warning(string code, string message, string field)
    {
        return new IssueFinding(code, message, field, new IssueWarningSeverity());
    }

    public static IssueFinding From(DomainFinding finding)
    {
        return finding.IsBlocking
            ? Critical(finding.Code, finding.Message, finding.Field)
            : Warning(finding.Code, finding.Message, finding.Field);
    }
}
