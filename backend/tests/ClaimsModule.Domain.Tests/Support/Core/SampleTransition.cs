using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

public abstract class SampleTransition(SampleState target) : Transition<SampleSubject, SampleState, SampleInput>(target)
{
    protected override SampleState Apply(SampleSubject subject, SampleInput input)
    {
        subject.Log.Add($"apply {input.Text} from {subject.State.Name}");
        subject.State = Target;

        return subject.State;
    }
}
