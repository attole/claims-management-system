using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Engine.Operations;

namespace ClaimsModule.Domain.Claims.Loss.Operations;

public sealed record UpdateLossDetailsInput(LossDetails Loss, DateTimeOffset Now) : OperationInput(Now);

public sealed class UpdateLossDetailsOperation : Operation<Claim, UpdateLossDetailsInput, Success>
{
    protected override Success Apply(Claim claim, UpdateLossDetailsInput input)
    {
        claim.UpdateLossDetails(input.Loss, input.Now);

        return Success.Value;
    }
}
