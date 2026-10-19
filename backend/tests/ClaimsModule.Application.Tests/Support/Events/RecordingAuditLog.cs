using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Events.Contracts;

namespace ClaimsModule.Application.Tests.Support.Events;

public sealed class RecordingAuditLog : IAuditLogService
{
    public List<AuditEntry> Entries { get; } = [];

    public Task WriteAsync(AuditEntry entry, EventContext context, CancellationToken cancellationToken)
    {
        Entries.Add(entry);

        return Task.CompletedTask;
    }
}
