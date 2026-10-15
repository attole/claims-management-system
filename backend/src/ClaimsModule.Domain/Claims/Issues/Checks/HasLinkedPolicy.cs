using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Issues.Checks;

internal sealed class HasLinkedPolicy : ICheck<Claim, DomainFinding>
{
    public const string Code = "S:INTAKE-NoPolicy";

    private const string Message = "No policy linked. Policy must be associated before reserves can be set.";
    private const string Field = "PolicyId";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasLinkedPolicy = claim.PolicyId != Guid.Empty;

        return hasLinkedPolicy
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field).AsWarning());
    }
}
