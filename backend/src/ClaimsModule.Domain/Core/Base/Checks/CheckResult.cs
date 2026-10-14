namespace ClaimsModule.Domain.Core.Base.Checks;

public sealed record CheckResult<TFinding>(IReadOnlyList<TFinding> Findings)
    where TFinding : IFinding
{
    public static CheckResult<TFinding> Pass { get; } = new([]);

    public bool HasNoFindings => Findings.Count == 0;

    public bool IsBlocked => Findings.Any(finding => finding.IsBlocking);

    public IReadOnlyList<TFinding> Blocking => Findings
        .Where(finding => finding.IsBlocking)
        .ToList();

    public IReadOnlyList<TFinding> Advisory => Findings
        .Where(finding => !finding.IsBlocking)
        .ToList();

    public static CheckResult<TFinding> Found(TFinding finding)
    {
        return new CheckResult<TFinding>([finding]);
    }

    public static CheckResult<TFinding> Combine(IEnumerable<CheckResult<TFinding>> results)
    {
        var findings = results
            .SelectMany(result => result.Findings)
            .ToList();

        return new CheckResult<TFinding>(findings);
    }
}
