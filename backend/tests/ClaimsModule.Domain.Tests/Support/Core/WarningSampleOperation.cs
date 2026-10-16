namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class WarningSampleOperation : SampleOperation<WarningSampleOperation>
{
    public const string Warned = "W";

    public WarningSampleOperation()
    {
        Require(SampleRules.Warning(Warned));
    }
}
