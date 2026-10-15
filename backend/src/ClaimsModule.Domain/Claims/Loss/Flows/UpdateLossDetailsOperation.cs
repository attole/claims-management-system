using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Loss.Flows;

internal sealed class UpdateLossDetailsOperation
    : Operation<UpdateLossDetailsOperation, ClaimLossEvent, ChangeLossInput, ClaimLossEvent>
{
    public UpdateLossDetailsOperation()
    {
        Require(change => new IsLossCompleteAfterDraft(change));
    }

    protected override ClaimLossEvent Apply(ClaimLossEvent loss, ChangeLossInput change)
    {
        loss.Update(change.Details, change.Now);

        return loss;
    }
}
