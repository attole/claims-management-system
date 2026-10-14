using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

public abstract class SampleTransition(SampleState target) : Transition<SampleSubject, SampleState, SampleInput>(target)
{
    protected override SampleState CurrentState(SampleSubject subject)
    {
        return subject.State;
    }

    protected override void Move(SampleSubject subject, SampleState state, SampleInput input)
    {
        subject.Log.Add($"move to {state.Name}");
        subject.State = state;
    }

    protected override void AfterMove(SampleSubject subject, SampleInput input)
    {
        subject.Log.Add("after move");
    }

    protected override Success Apply(SampleSubject subject, SampleInput input)
    {
        subject.Log.Add($"apply {input.Text} from {subject.State.Name}");

        return Success.Value;
    }
}
