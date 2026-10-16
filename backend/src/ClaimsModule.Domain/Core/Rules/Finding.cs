namespace ClaimsModule.Domain.Core.Rules;

public sealed record Finding(string Code, string Message, string Field = "")
{
    public FindingSeverity Severity { get; init; } = FindingSeverity.Critical;

    public string Evidence { get; init; } = string.Empty;

    public bool HasCode(string code)
    {
        bool hasCode = string.Equals(Code, code, StringComparison.Ordinal);

        return hasCode;
    }

    public bool IsSameProblem(Finding other)
    {
        bool hasSameEvidence = string.Equals(Evidence, other.Evidence, StringComparison.Ordinal);
        bool isSameProblem = HasCode(other.Code) && hasSameEvidence;

        return isSameProblem;
    }

    public Finding AsWarning()
    {
        return this with { Severity = FindingSeverity.Warning };
    }
}
