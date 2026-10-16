using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasLinkedPolicy : IRule<Claim>
{
    public const string Code = "S:INTAKE-NoPolicy";

    private const string Message = "No policy linked. Policy must be associated before reserves can be set.";
    private const string Field = "PolicyId";

    public CheckResult Check(Claim claim)
    {
        bool hasLinkedPolicy = claim.HasLinkedPolicy;

        return hasLinkedPolicy
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field).AsWarning());
    }
}
