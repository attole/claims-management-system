using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasPolicyId(Guid policyId) : IRule<object>
{
    public const string Code = "POLICY_ID_REQUIRED";

    private const string Message = "A policy id is required to link a policy.";
    private const string Field = "PolicyId";

    public CheckResult Check(object subject)
    {
        bool hasPolicyId = policyId != Guid.Empty;

        return hasPolicyId
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
