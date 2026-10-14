using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

public sealed class HasNoBlockingIssues : IRule<Claim>
{
    public const string Code = "S:BR-ST-02";

    private const string Message = "The claim has unresolved Critical issues.";
    private const string Field = "Issues";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasNoBlockingIssues = claim.BlockingIssues.Count == 0;

        return hasNoBlockingIssues
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
