using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Rules;

public sealed class IsPolicyLinked(bool isLinked) : IRule<NoSubject>
{
    public const string Code = "S:BR-C-06";

    private const string Message = "No policy linked. Policy must be associated before reserves can be set.";
    private const string Field = "PolicyId";

    public CheckResult<DomainFinding> Check(NoSubject subject)
    {
        return isLinked
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
