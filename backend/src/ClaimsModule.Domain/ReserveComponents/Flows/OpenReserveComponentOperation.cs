using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents.Inputs;
using ClaimsModule.Domain.ReserveComponents.Rules;

namespace ClaimsModule.Domain.ReserveComponents.Flows;

internal sealed class OpenReserveComponentOperation
    : CreationOperation<OpenReserveComponentOperation, OpenReserveComponentInput, ReserveComponent>
{
    public OpenReserveComponentOperation()
    {
        Require(input => new IsClaimAcceptingReserves(input.Claim.State));
    }

    protected override Outcome<ReserveComponent> Apply(OpenReserveComponentInput input)
    {
        var component = new ReserveComponent(
            Ids.Next(),
            input.Claim.ClaimId,
            input.Claim.OrganisationId,
            input.ComponentType,
            input.Notes);

        return Done(component);
    }
}
