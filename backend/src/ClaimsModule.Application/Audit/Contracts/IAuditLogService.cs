using ClaimsModule.Application.Events.Contracts;

namespace ClaimsModule.Application.Audit.Contracts;

public interface IAuditLogService
{
    Task WriteAsync(AuditEntry entry, EventContext context, CancellationToken cancellationToken);
}
