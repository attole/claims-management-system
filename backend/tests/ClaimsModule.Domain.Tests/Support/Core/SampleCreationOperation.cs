using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class SampleCreationOperation : CreationOperation<SampleCreationOperation, SampleInput, string>
{
    protected override Outcome<string> Apply(SampleInput input)
    {
        return Done(input.Text);
    }
}
