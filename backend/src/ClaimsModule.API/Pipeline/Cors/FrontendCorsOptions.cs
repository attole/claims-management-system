using System.ComponentModel.DataAnnotations;

namespace ClaimsModule.API.Pipeline.Cors;

public sealed class FrontendCorsOptions
{
    public const string SectionName = "Cors";

    [MinLength(1)]
    public string[] AllowedOrigins { get; init; } = [];
}
