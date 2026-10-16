namespace ClaimsModule.Domain.Claims.Issues.Inputs;

internal sealed record IssueResolutionInput(Guid ActorId, DateTimeOffset Now);
