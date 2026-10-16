using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal abstract class SampleOperation<TSelf> : Operation<TSelf, SampleSubject, SampleInput, string>
    where TSelf : SampleOperation<TSelf>, new()
{
    protected override Outcome<string> Apply(SampleSubject subject, SampleInput input)
    {
        subject.Log.Add($"apply {input.Text}");

        return Done(input.Text);
    }
}
