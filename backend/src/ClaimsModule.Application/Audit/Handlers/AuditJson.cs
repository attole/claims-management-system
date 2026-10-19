using System.Text.Json;

namespace ClaimsModule.Application.Audit.Handlers;

public static class AuditJson
{
    public static string From(object value)
    {
        return JsonSerializer.Serialize(value);
    }
}
