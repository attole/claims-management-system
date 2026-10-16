namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class FailingSampleOperation : SampleOperation<FailingSampleOperation>
{
    public const string FirstFailure = "A";
    public const string SecondFailure = "B";

    public FailingSampleOperation()
    {
        Require(SampleRules.Critical(FirstFailure));
        Require(SampleRules.Pass());
        Require(SampleRules.Critical(SecondFailure));
    }
}
