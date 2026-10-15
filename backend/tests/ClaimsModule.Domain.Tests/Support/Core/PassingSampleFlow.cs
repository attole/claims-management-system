namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class PassingSampleFlow : SampleFlow<PassingSampleFlow>
{
    public PassingSampleFlow()
    {
        Require(SampleRules.Pass());
    }
}
