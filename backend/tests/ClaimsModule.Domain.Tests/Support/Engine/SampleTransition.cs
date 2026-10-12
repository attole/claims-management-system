using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Engine.Transitions;
using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Engine;

public abstract class SampleTransition(SampleState target) : Transition<SampleSubject, SampleState, SampleInput>(target)
{
    protected override SampleState CurrentState(SampleSubject subject)
    {
        return subject.State;
    }

    protected override void Move(SampleSubject subject, SampleState state)
    {
        subject.State = state;
    }

    protected override Success Apply(SampleSubject subject, SampleInput input)
    {
        subject.Log.Add($"apply {input.Text}");

        return Success.Value;
    }
}
