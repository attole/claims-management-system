using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal abstract class SampleOperation<TSelf> : Operation<TSelf, SampleSubject, SampleInput, string>
    where TSelf : SampleOperation<TSelf>, new()
{
    protected override string Apply(SampleSubject subject, SampleInput input)
    {
        subject.Log.Add($"apply {input.Text}");

        return input.Text;
    }
}
