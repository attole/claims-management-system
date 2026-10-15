namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class TimeSampleOperation : SampleOperation<TimeSampleOperation>
{
    public TimeSampleOperation()
    {
        Require(input => SampleRules.Log(input.Now.ToString("O")));
    }
}
