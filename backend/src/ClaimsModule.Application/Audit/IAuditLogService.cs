using ClaimsModule.Application.Events;

namespace ClaimsModule.Application.Audit;

public interface IAuditLogService
{
    Task WriteAsync(AuditEntry entry, EventContext context, CancellationToken cancellationToken);
}
