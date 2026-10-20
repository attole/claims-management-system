namespace ClaimsModule.Application.ReserveComponents.Contracts;

public sealed record UnpostedReserve(Guid OrganisationId, Guid ClaimId, Guid ReserveHistoryId);
