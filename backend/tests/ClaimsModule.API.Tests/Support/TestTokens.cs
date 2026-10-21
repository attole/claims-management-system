using System.Text;
using ClaimsModule.API.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ClaimsModule.API.Tests.Support;

public static class TestTokens
{
    public const string SigningKey = "test-signing-key-for-the-api-tests-0123456789";
    public const string Issuer = "claims-module-tests";
    public const string Audience = "claims-module-tests-frontend";

    public static string Create(Guid userId, Guid organisationId, string role)
    {
        return Create(SigningKey, new Dictionary<string, object>
        {
            [IdentityClaims.Subject] = userId.ToString(),
            [IdentityClaims.Organisation] = organisationId.ToString(),
            [IdentityClaims.Role] = role,
            [IdentityClaims.Name] = "Test User",
        });
    }

    public static string Create(string signingKey, IDictionary<string, object> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Claims = claims,
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
