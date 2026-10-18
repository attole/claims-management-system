using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;

namespace ClaimsModule.Domain.ReserveComponents.History.Flows;

internal sealed class MarkHistoryPostedOnReserveComponentOperation
    : Operation<
        MarkHistoryPostedOnReserveComponentOperation,
        ReserveComponent,
        HistoryPostingInput,
        Move<ReservePostingState>>
{
    protected override Outcome<Move<ReservePostingState>> Apply(ReserveComponent component, HistoryPostingInput input)
    {
        var history = component.History.Single(history => history.Id == input.ReserveHistoryId);
        var posting = new RowPostingInput
        {
            JobId = input.JobId,
            FailureReason = input.FailureReason,
        };

        return history.MarkPosted(posting);
    }
}
