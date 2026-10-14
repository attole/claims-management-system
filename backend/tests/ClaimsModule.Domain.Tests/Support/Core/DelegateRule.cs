using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Support.Core;

public sealed class DelegateRule(Func<SampleSubject, CheckResult<DomainFinding>> check) : IRule<SampleSubject>
{
    public CheckResult<DomainFinding> Check(SampleSubject subject)
    {
        return check(subject);
    }
}
