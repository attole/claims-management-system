namespace ClaimsModule.Application.Audit.Contracts;

public interface IAuditLogService
{
    Task WriteAsync(AuditEntry entry, AuditContext context, CancellationToken cancellationToken);
}
