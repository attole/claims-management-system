using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

public sealed class HasLiabilityConfirmation(bool isConfirmed) : IRule<Claim>
{
    public const string Code = "LIABILITY_CONFIRMATION_REQUIRED";

    private const string Message = "Liability must be confirmed to move the claim to pending payment.";
    private const string Field = "IsLiabilityConfirmed";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasLiabilityConfirmation = isConfirmed;

        return hasLiabilityConfirmation
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
