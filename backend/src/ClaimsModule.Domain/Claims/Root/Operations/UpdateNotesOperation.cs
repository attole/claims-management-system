using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Operations;

public sealed record UpdateNotesInput(string Notes, Guid ActorId, DateTimeOffset Now, ClaimReferenceSnapshot Reference)
    : ClaimBaseOperationInput(Now, ActorId, Reference);

internal sealed class UpdateNotesOperation : Operation<Claim, UpdateNotesInput, Success>
{
    public UpdateNotesOperation()
    {
        Require(new IsClaimNotWithdrawn());
    }

    protected override Success Apply(Claim claim, UpdateNotesInput input)
    {
        claim.Notes = input.Notes;

        return Success.Value;
    }
}
