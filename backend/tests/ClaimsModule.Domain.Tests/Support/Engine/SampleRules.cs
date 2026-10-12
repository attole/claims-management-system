using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Engine.Rules;

namespace ClaimsModule.Domain.Tests.Support.Engine;

public static class SampleRules
{
    public static DelegateRule Pass()
    {
        return new DelegateRule(_ => RuleResult.Pass);
    }

    public static DelegateRule Fail(string code)
    {
        return new DelegateRule(_ => RuleResult.Fail(new RuleViolation(code, code)));
    }

    public static DelegateRule Warn(string code)
    {
        return new DelegateRule(_ => RuleResult.Warn(new RuleViolation(code, code)));
    }
}
