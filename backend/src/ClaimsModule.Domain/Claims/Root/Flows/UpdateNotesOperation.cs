using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Flows;

internal sealed class UpdateNotesOperation : Operation<UpdateNotesOperation, Claim, UpdateNotesInput, Success>
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
