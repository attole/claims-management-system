using ClaimsModule.Domain.Engine.Rules;

namespace ClaimsModule.Domain.Tests.Support.Engine;

public sealed class DelegateRule(Func<SampleSubject, RuleResult> check) : IRule<SampleSubject>
{
    public RuleResult Check(SampleSubject subject)
    {
        return check(subject);
    }
}
