using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Reserves.Inputs;
using ClaimsModule.Domain.Reserves.Rules;

namespace ClaimsModule.Domain.Reserves.Flows;

internal sealed class OpenReserveOperation : CreationOperation<OpenReserveOperation, OpenReserveInput, Reserve>
{
    public OpenReserveOperation()
    {
        Require(input => new IsClaimAcceptingReserves(input.IsClaimEditable));
    }

    protected override Outcome<Reserve> Apply(OpenReserveInput input)
    {
        var reserve = new Reserve(Ids.Next(), input.OrganisationId, input.ClaimId, input.ComponentType, input.Notes);

        return Done(reserve);
    }
}
