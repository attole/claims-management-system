using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class DelegateSampleRule(Func<SampleSubject, CheckResult> check) : IRule<SampleSubject>
{
    public CheckResult Check(SampleSubject subject)
    {
        return check(subject);
    }
}
