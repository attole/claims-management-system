using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Operations;

public sealed record CreateClaimInput(
    Guid OrganisationId,
    ClaimNumber ClaimNumber,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseOperationInput(Now, ActorId, Reference);

internal sealed class CreateClaimOperation : CreationOperation<CreateClaimInput, Claim>
{
    public CreateClaimOperation()
    {
        Require(input => new HasOrganisation(input.OrganisationId));
        Require(input => new HasClaimNumber(input.ClaimNumber));
    }

    protected override Claim Apply(CreateClaimInput input)
    {
        return new Claim(Ids.Next(input.Now), input.OrganisationId, input.ClaimNumber, input.Now);
    }
}
