namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class TextSampleOperation : SampleOperation<TextSampleOperation>
{
    public const string BadText = "bad";
    public const string BadInput = "BAD_INPUT";

    public TextSampleOperation()
    {
        Require(input => input.Text == BadText ? SampleRules.Fail(BadInput) : SampleRules.Pass());
    }
}
