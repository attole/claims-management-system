namespace ClaimsModule.Domain.Reserves.Root.Inputs;

public sealed record OpenReserveInput(
    Guid OrganisationId,
    Guid ClaimId,
    ReserveComponentType ComponentType,
    string Notes,
    Guid ActorId,
    DateTimeOffset Now) : ReserveBaseOperationInput(Now, ActorId);
