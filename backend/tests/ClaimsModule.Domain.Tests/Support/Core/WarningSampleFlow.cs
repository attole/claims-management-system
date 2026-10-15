namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class WarningSampleFlow : SampleFlow<WarningSampleFlow>
{
    public const string Warned = "WARN";

    public WarningSampleFlow()
    {
        Require(SampleRules.Warn(Warned));
    }
}
