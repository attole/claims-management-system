namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class MixedSampleOperation : SampleOperation<MixedSampleOperation>
{
    public const string Warned = "W";
    public const string Failed = "E";

    public MixedSampleOperation()
    {
        Require(SampleRules.Warning(Warned));
        Require(SampleRules.Critical(Failed));
    }
}
