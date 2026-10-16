using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasLiabilityConfirmation(bool isConfirmed) : IRule<object>
{
    public const string Code = "LIABILITY_CONFIRMATION_REQUIRED";

    private const string Message = "Liability must be confirmed to move the claim to pending payment.";
    private const string Field = "IsLiabilityConfirmed";

    public CheckResult Check(object subject)
    {
        bool hasLiabilityConfirmation = isConfirmed;

        return hasLiabilityConfirmation
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
