using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasNoCriticalIssuesForOpen : IRule<Claim>
{
    public const string Code = "S:BR-ST-02";

    private const string Message = "The claim has unresolved Critical issues.";
    private const string Field = "Issues";

    public CheckResult Check(Claim claim)
    {
        bool hasNoCriticalIssues = !claim.HasUnresolvedCriticalIssues;

        return hasNoCriticalIssues
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
