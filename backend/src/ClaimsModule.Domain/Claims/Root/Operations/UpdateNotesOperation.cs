using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Engine.Operations;

namespace ClaimsModule.Domain.Claims.Root.Operations;

public sealed record UpdateNotesInput(string Notes, DateTimeOffset Now) : OperationInput(Now);

public sealed class UpdateNotesOperation : Operation<Claim, UpdateNotesInput, Success>
{
    protected override Success Apply(Claim claim, UpdateNotesInput input)
    {
        claim.UpdateNotes(input.Notes, input.Now);

        return Success.Value;
    }
}
