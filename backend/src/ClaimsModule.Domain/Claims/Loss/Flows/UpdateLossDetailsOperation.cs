using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Loss.Flows;

internal sealed class UpdateLossDetailsOperation
    : Operation<UpdateLossDetailsOperation, LossEvent, LossDetailsInput, LossEvent>
{
    protected override Outcome<LossEvent> Apply(LossEvent loss, LossDetailsInput input)
    {
        loss.Details = input.Details;

        return Done(loss);
    }
}
