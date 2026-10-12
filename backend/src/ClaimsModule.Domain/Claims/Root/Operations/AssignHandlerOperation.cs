using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Engine.Operations;

namespace ClaimsModule.Domain.Claims.Root.Operations;

public sealed record AssignHandlerInput(Guid HandlerId, DateTimeOffset Now) : OperationInput(Now);

public sealed class AssignHandlerOperation : Operation<Claim, AssignHandlerInput, Success>
{
    protected override Success Apply(Claim claim, AssignHandlerInput input)
    {
        claim.AssignHandler(input.HandlerId, input.Now);

        return Success.Value;
    }
}
