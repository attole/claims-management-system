namespace ClaimsModule.Domain.Claims;

public sealed record ClaimReference(Guid ClaimId, Guid OrganisationId, ClaimState State);
