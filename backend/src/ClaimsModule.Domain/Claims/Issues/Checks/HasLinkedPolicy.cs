using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;

namespace ClaimsModule.Domain.Claims.Issues.Checks;

internal sealed class HasLinkedPolicy : ICheck<Claim, IssueFinding>
{
    public const string Code = "S:INTAKE-NoPolicy";

    private const string Message = "No policy linked. Policy must be associated before reserves can be set.";
    private const string Field = "PolicyId";

    public CheckResult<IssueFinding> Check(Claim claim)
    {
        bool hasLinkedPolicy = claim.PolicyId != Guid.Empty;

        return hasLinkedPolicy
            ? CheckResult<IssueFinding>.Pass
            : CheckResult<IssueFinding>.Found(IssueFinding.Warning(Code, Message, Field));
    }
}
