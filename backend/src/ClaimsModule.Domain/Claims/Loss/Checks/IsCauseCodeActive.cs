using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Checks;

internal sealed class IsCauseCodeActive(IReadOnlyCollection<string> activeCodes) : ICheck<LossDetails, DomainFinding>
{
    public const string Code = "S:BR-C-05";

    private const string Message = "Cause of loss code is not recognised or is inactive.";
    private const string Field = "CauseOfLossCode";

    public CheckResult<DomainFinding> Check(LossDetails loss)
    {
        bool isActive = activeCodes.Any(active => IsSameCode(active, loss.CauseOfLossCode));

        return isActive
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }

    private static bool IsSameCode(string first, string second)
    {
        return string.Equals(first, second, StringComparison.OrdinalIgnoreCase);
    }
}
