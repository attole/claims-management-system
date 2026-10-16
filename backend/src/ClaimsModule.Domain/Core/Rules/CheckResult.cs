namespace ClaimsModule.Domain.Core.Rules;

internal sealed record CheckResult(IReadOnlyList<Finding> Findings)
{
    public static CheckResult Pass { get; } = new([]);

    public bool HasCriticalFindings => Findings.Any(finding => finding.Severity == FindingSeverity.Critical);

    public IReadOnlyList<Finding> CriticalFindings => Findings
        .Where(finding => finding.Severity == FindingSeverity.Critical)
        .ToList();

    public IReadOnlyList<Finding> WarningFindings => Findings
        .Where(finding => finding.Severity == FindingSeverity.Warning)
        .ToList();

    public static CheckResult Found(Finding finding)
    {
        return new CheckResult([finding]);
    }

    public static CheckResult Combine(IEnumerable<CheckResult> results)
    {
        var findings = results
            .SelectMany(result => result.Findings)
            .ToList();

        return new CheckResult(findings);
    }
}
