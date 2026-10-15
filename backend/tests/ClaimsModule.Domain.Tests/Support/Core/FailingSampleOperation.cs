namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class FailingSampleOperation : SampleOperation<FailingSampleOperation>
{
    public const string FirstFailure = "A";
    public const string SecondFailure = "B";

    public FailingSampleOperation()
    {
        Require(SampleRules.Fail(FirstFailure));
        Require(SampleRules.Pass());
        Require(SampleRules.Fail(SecondFailure));
    }
}
