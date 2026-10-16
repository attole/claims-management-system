using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasNoCriticalIssuesForClose : IRule<Claim>
{
    public const string Code = "S:CC-02";

    private const string Message = "Claim cannot be closed - clearing of every Critical issue is not satisfied.";
    private const string Field = "Issues";

    public CheckResult Check(Claim claim)
    {
        bool hasNoCriticalIssues = !claim.HasUnresolvedCriticalIssues;

        return hasNoCriticalIssues
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
