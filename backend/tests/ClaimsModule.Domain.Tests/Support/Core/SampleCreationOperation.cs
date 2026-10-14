using ClaimsModule.Domain.Core.Operations.Creation;

namespace ClaimsModule.Domain.Tests.Support.Core;

public sealed class SampleCreationOperation : CreationOperation<SampleInput, string>
{
    public List<string> Log { get; } = [];

    protected override string Apply(SampleInput input)
    {
        Log.Add($"apply {input.Text}");

        return input.Text;
    }

    protected override void AfterCreate(string value, SampleInput input)
    {
        Log.Add($"after {value}");
    }
}
