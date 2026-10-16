using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Support.Claims;

internal sealed class CriticalSampleClaimRule : IRule<Claim>
{
    public const string Code = "CLAIM_RULE_FAILED";

    public CheckResult Check(Claim claim)
    {
        return CheckResult.Found(new Finding(Code, Code));
    }
}
