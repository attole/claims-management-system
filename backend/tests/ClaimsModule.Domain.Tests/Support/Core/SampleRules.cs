using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal static class SampleRules
{
    public static DelegateSampleRule Pass()
    {
        return new DelegateSampleRule(_ => CheckResult.Pass);
    }

    public static DelegateSampleRule Critical(string code)
    {
        return new DelegateSampleRule(_ => CheckResult.Found(new Finding(code, code)));
    }

    public static DelegateSampleRule Warning(string code)
    {
        return new DelegateSampleRule(_ => CheckResult.Found(new Finding(code, code).AsWarning()));
    }
}
