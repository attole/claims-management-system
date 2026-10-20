using ClaimsModule.Application.Audit.Contracts;

namespace ClaimsModule.Application.Tests.Support.Events;

public sealed class RecordingAuditLog : IAuditLogService
{
    public List<AuditEntry> Entries { get; } = [];

    public List<AuditContext> Contexts { get; } = [];

    public Task WriteAsync(AuditEntry entry, AuditContext context, CancellationToken cancellationToken)
    {
        Entries.Add(entry);
        Contexts.Add(context);

        return Task.CompletedTask;
    }
}
