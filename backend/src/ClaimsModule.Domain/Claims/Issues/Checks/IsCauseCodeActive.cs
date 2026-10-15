using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;

namespace ClaimsModule.Domain.Claims.Issues.Checks;

internal sealed class IsCauseCodeActive(IReadOnlyCollection<string> activeCodes) : ICheck<Claim, IssueFinding>
{
    public const string Code = "S:BR-C-05";

    private const string Message = "Cause of loss code is not recognised or is inactive.";
    private const string Field = "CauseOfLossCode";

    public CheckResult<IssueFinding> Check(Claim claim)
    {
        string code = claim.LossEvent.Details.CauseOfLossCode;
        bool isActive = activeCodes.Any(active => string.Equals(active, code, StringComparison.OrdinalIgnoreCase));

        return isActive
            ? CheckResult<IssueFinding>.Pass
            : CheckResult<IssueFinding>.Found(IssueFinding.Critical(Code, Message, Field));
    }
}
