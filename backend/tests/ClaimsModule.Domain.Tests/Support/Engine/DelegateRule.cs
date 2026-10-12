using ClaimsModule.Domain.Engine.Operations;
using ClaimsModule.Domain.Engine.Rules;

namespace ClaimsModule.Domain.Tests.Support.Engine;

public sealed class DelegateRule(Func<SampleSubject, OperationContext, RuleResult> check) : IRule<SampleSubject>
{
    public RuleResult Check(SampleSubject subject, OperationContext context)
    {
        return check(subject, context);
    }
}
