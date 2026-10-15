using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.Reserves.Root.Bases;

namespace ClaimsModule.Domain.Reserves.Root.Operations;

public sealed record OpenReserveInput(
    Guid OrganisationId,
    Guid ClaimId,
    ReserveComponentType ComponentType,
    string Notes,
    Guid ActorId,
    DateTimeOffset Now) : ReserveBaseOperationInput(Now, ActorId);

public sealed class OpenReserveOperation : CreationOperation<OpenReserveInput, Reserve>
{
    protected override Reserve Apply(OpenReserveInput input)
    {
        return Reserve.Create(
            input.OrganisationId,
            input.ClaimId,
            input.ComponentType,
            input.Notes,
            input.Now);
    }
}
