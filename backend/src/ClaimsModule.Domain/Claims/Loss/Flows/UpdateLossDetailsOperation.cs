using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Loss.Flows;

internal sealed class UpdateLossDetailsOperation
    : Operation<UpdateLossDetailsOperation, ClaimLossEvent, LossDetailsInput, ClaimLossEvent>
{
    protected override Outcome<ClaimLossEvent> Apply(ClaimLossEvent loss, LossDetailsInput input)
    {
        loss.Details = input.Details;

        return Done(loss);
    }
}
