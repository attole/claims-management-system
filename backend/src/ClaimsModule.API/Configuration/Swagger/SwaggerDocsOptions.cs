namespace ClaimsModule.API.Configuration.Swagger;

public sealed class SwaggerDocsOptions
{
    public const string SectionName = "Swagger";

    public bool Enabled { get; init; } = false;
}
