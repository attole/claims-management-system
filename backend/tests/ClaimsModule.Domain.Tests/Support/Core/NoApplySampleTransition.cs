using ClaimsModule.Domain.Core.Transitions;
using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

public sealed class NoApplySampleTransition : Transition<SampleSubject, SampleState, SampleInput>
{
    public NoApplySampleTransition()
        : base(new OpenSampleState())
    {
        AllowedFrom(new DraftSampleState());
    }

    protected override SampleState CurrentState(SampleSubject subject)
    {
        return subject.State;
    }

    protected override void Move(SampleSubject subject, SampleState state, SampleInput input)
    {
        subject.State = state;
    }
}
