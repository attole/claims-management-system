namespace ClaimsModule.Application.Audit.Contracts;

public record AuditContext(DateTimeOffset OccurredAt, Guid ActorId, Guid CorrelationId);
