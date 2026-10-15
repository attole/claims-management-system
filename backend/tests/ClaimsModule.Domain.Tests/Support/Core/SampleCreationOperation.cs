using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class SampleCreationOperation : CreationOperation<SampleCreationOperation, SampleInput, string>
{
    protected override string Apply(SampleInput input)
    {
        return input.Text;
    }
}
