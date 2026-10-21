using System.ComponentModel.DataAnnotations;

namespace ClaimsModule.API.Cors;

public sealed class FrontendCorsOptions
{
    public const string SectionName = "Cors";

    [MinLength(1)]
    public string[] AllowedOrigins { get; init; } = [];
}
