using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal abstract class SampleTransition<TSelf>(SampleState target)
    : Transition<TSelf, SampleSubject, SampleState, SampleInput>(target)
    where TSelf : SampleTransition<TSelf>, new()
{
    protected override StateChange<SampleState> Apply(SampleSubject subject, SampleInput input)
    {
        var change = new StateChange<SampleState>(subject.State, Target);
        subject.Log.Add($"apply {input.Text} from {subject.State.Name}");
        subject.State = Target;

        return change;
    }
}
