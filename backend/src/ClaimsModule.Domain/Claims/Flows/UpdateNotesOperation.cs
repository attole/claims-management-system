using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Flows;

internal sealed class UpdateNotesOperation : Operation<UpdateNotesOperation, Claim, UpdateNotesInput, Claim>
{
    protected override Outcome<Claim> Apply(Claim claim, UpdateNotesInput input)
    {
        claim.Notes = input.Notes;

        return Done(claim);
    }
}
