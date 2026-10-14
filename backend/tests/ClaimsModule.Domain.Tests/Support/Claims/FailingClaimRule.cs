using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Support.Claims;

public sealed class FailingClaimRule : IRule<Claim>
{
    public const string Code = "CLAIM_RULE_FAILED";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        return CheckResult<DomainFinding>.Found(new RuleFinding(Code, Code));
    }
}
