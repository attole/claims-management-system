using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Operations;

public sealed record AssignHandlerInput(
    Guid HandlerId,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseOperationInput(Now, ActorId, Reference);

internal sealed class AssignHandlerOperation : Operation<Claim, AssignHandlerInput, Success>
{
    public AssignHandlerOperation()
    {
        Require(new IsClaimEditable());
    }

    protected override Success Apply(Claim claim, AssignHandlerInput input)
    {
        claim.AssignedHandlerId = input.HandlerId;

        return Success.Value;
    }
}
