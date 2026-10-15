using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.Reserves.Root.Inputs;

namespace ClaimsModule.Domain.Reserves.Root.Flows;

internal sealed class OpenReserveOperation : CreationOperation<OpenReserveOperation, OpenReserveInput, Reserve>
{
    protected override Reserve Apply(OpenReserveInput input)
    {
        return new Reserve(
            Ids.Next(input.Now),
            input.OrganisationId,
            input.ClaimId,
            input.ComponentType,
            input.Notes,
            input.Now);
    }
}
