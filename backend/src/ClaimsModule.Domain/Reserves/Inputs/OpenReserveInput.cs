namespace ClaimsModule.Domain.Reserves.Inputs;

public sealed record OpenReserveInput(
    Guid OrganisationId,
    Guid ClaimId,
    ReserveComponentType ComponentType,
    string Notes,
    bool IsClaimEditable);
