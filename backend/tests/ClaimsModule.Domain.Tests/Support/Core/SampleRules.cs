using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal static class SampleRules
{
    public static DelegateRule Pass()
    {
        return new DelegateRule(_ => CheckResult<DomainFinding>.Pass);
    }

    public static DelegateRule Fail(string code)
    {
        return new DelegateRule(_ => CheckResult<DomainFinding>.Found(new RuleFinding(code, code)));
    }

    public static DelegateRule Warn(string code)
    {
        return new DelegateRule(_ => CheckResult<DomainFinding>.Found(new RuleFinding(code, code).AsWarning()));
    }

    public static DelegateRule Log(string text)
    {
        return new DelegateRule(subject =>
        {
            subject.Log.Add(text);

            return CheckResult<DomainFinding>.Pass;
        });
    }
}
