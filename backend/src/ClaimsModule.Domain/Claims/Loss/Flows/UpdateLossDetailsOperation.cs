using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Loss.Flows;

internal sealed class UpdateLossDetailsOperation
    : Operation<UpdateLossDetailsOperation, Claim, UpdateLossDetailsInput, LossEvent>
{
    public UpdateLossDetailsOperation()
    {
        Require(new IsClaimEditable());
    }

    protected override LossEvent Apply(Claim claim, UpdateLossDetailsInput input)
    {
        claim.LossEvent.Update(input.Loss, input.Now);

        return claim.LossEvent;
    }
}
