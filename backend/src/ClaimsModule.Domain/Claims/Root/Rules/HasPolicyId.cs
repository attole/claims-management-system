using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

public sealed class HasPolicyId(Guid policyId) : IRule<Claim>
{
    public const string Code = "POLICY_ID_REQUIRED";

    private const string Message = "A policy id is required to link a policy.";
    private const string Field = "PolicyId";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasPolicyId = policyId != Guid.Empty;

        return hasPolicyId
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
