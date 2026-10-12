using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Engine.Rules;

public sealed record RuleResult(IReadOnlyList<DomainError> Errors, IReadOnlyList<DomainError> Warnings)
{
    public static RuleResult Pass { get; } = new([], []);

    public static RuleResult Fail(DomainError error)
    {
        return new RuleResult([error], []);
    }

    public static RuleResult Warn(DomainError warning)
    {
        return new RuleResult([], [warning]);
    }

    public static RuleResult Combine(IEnumerable<RuleResult> results)
    {
        List<RuleResult> all = results.ToList();

        List<DomainError> errors = all.SelectMany(result => result.Errors).ToList();
        List<DomainError> warnings = all.SelectMany(result => result.Warnings).ToList();

        return new RuleResult(errors, warnings);
    }
}
