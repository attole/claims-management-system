namespace ClaimsModule.API.Tests.Support.TestEndpoints;

public sealed record WhoAmIDto(Guid Id, Guid OrganisationId, string Role, Guid CorrelationId);
