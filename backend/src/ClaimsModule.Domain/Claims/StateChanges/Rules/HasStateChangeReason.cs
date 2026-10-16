using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.StateChanges.Rules;

internal sealed class HasStateChangeReason(StateChangeReason reason) : IRule<object>
{
    public const string Code = "STATE_CHANGE_REASON_REQUIRED";

    private const string Message = "A reason with a code and a text is required.";
    private const string Field = "Reason";

    public CheckResult Check(object subject)
    {
        bool hasReason = reason.IsComplete;

        return hasReason
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
