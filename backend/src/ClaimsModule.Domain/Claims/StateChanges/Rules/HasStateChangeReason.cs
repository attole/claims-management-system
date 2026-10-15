using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.StateChanges.Rules;

internal sealed class HasStateChangeReason(StateChangeReason reason) : IRule<Claim>
{
    public const string Code = "STATE_CHANGE_REASON_REQUIRED";

    private const string Message = "A reason with a code and a text is required.";
    private const string Field = "Reason";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasReason = reason.IsComplete;

        return hasReason
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
