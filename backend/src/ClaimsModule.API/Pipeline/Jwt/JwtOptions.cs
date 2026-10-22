using System.ComponentModel.DataAnnotations;

namespace ClaimsModule.API.Pipeline.Jwt;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    [MinLength(32)]
    [RegularExpression(
        @"^(?!@Microsoft\.KeyVault).*$",
        ErrorMessage = "Jwt:SigningKey is an unresolved Key Vault reference.")]
    public string SigningKey { get; init; } = string.Empty;
}
