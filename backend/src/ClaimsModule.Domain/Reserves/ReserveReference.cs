namespace ClaimsModule.Domain.Reserves;

public sealed record ReserveReference(
    Guid ReserveId,
    Guid ClaimId,
    Guid OrganisationId,
    ReserveComponentType ComponentType);
