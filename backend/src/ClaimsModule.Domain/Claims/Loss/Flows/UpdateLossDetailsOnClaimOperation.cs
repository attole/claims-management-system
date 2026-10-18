using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Loss.Flows;

internal sealed class UpdateLossDetailsOnClaimOperation
    : Operation<UpdateLossDetailsOnClaimOperation, Claim, UpdateLossDetailsInput, LossEvent>
{
    public UpdateLossDetailsOnClaimOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new IsLossCompleteAfterDraft(input.Loss, input.PolicyPeriod, input.CauseCodes));
    }

    protected override Outcome<LossEvent> Apply(Claim claim, UpdateLossDetailsInput input)
    {
        return claim.LossEvent.Update(new LossDetailsInput(input.Loss));
    }
}
