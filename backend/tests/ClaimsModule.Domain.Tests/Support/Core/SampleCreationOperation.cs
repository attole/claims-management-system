using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Tests.Support.Core;

public sealed class SampleCreationOperation : CreationOperation<SampleInput, string>
{
    protected override string Apply(SampleInput input)
    {
        return input.Text;
    }
}
