namespace ClaimsModule.API.Pipeline.Telemetry;

public sealed class TelemetryOptions
{
    public const string SectionName = "Telemetry";

    public string ConnectionString { get; init; } = string.Empty;
}
