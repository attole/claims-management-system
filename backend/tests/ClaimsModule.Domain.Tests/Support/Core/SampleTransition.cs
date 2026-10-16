using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal abstract class SampleTransition<TSelf>(SampleState target)
    : Transition<TSelf, SampleSubject, SampleState, SampleInput>(target)
    where TSelf : SampleTransition<TSelf>, new()
{
    protected override void ApplyMove(SampleSubject subject, SampleInput input, Move<SampleState> move)
    {
        subject.Log.Add($"apply {input.Text} from {move.From.Name}");
        subject.State = move.To;
    }
}
