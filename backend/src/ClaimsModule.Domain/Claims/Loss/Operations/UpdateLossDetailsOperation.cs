using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Loss.Operations;

public sealed record UpdateLossDetailsInput(LossDetails Loss, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseOperationInput(Now, ActorId);

public sealed class UpdateLossDetailsOperation : ClaimBaseOperation<UpdateLossDetailsInput, Success>
{
    public UpdateLossDetailsOperation()
    {
        Require(new IsClaimEditable());
    }

    protected override Success Apply(Claim claim, UpdateLossDetailsInput input)
    {
        claim.UpdateLossDetails(input.Loss, input.Now);

        return Success.Value;
    }
}
