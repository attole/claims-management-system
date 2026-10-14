using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Operations;

public sealed record UpdateNotesInput(string Notes, Guid ActorId, DateTimeOffset Now) : ClaimBaseInput(Now, ActorId);

public sealed class UpdateNotesOperation : ClaimBaseOperation<UpdateNotesInput, Success>
{
    protected override Success Apply(Claim claim, UpdateNotesInput input)
    {
        claim.UpdateNotes(input.Notes, input.Now);

        return Success.Value;
    }
}
