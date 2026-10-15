namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class PassingSampleOperation : SampleOperation<PassingSampleOperation>
{
    public PassingSampleOperation()
    {
        Require(SampleRules.Pass());
        Require(SampleRules.Pass());
    }
}
