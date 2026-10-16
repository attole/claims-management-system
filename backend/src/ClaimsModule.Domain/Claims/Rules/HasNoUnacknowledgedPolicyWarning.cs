using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasNoUnacknowledgedPolicyWarning : IRule<Claim>
{
    public const string Code = "S:BR-C-02";

    private const string Message = "The policy period warning must be cleared or acknowledged before opening.";
    private const string Field = "Issues";

    public CheckResult Check(Claim claim)
    {
        bool hasNoUnacknowledgedWarning = !claim.HasUnresolvedWarning(IsLossDateWithinPolicyPeriod.Code);

        return hasNoUnacknowledgedWarning
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
