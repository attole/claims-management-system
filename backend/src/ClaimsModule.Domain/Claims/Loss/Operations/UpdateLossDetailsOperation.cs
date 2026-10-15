using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Loss.Operations;

public sealed record UpdateLossDetailsInput(
    LossDetails Loss,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseOperationInput(Now, ActorId, Reference);

internal sealed class UpdateLossDetailsOperation : Operation<Claim, UpdateLossDetailsInput, LossEvent>
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
