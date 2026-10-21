using System.Text;
using ClaimsModule.API.DevAuth.Configuration;
using ClaimsModule.API.DevAuth.Dtos;
using ClaimsModule.API.Pipeline.Identity;
using ClaimsModule.API.Pipeline.Jwt;
using ClaimsModule.Application.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ClaimsModule.API.DevAuth;

public sealed class DevTokenIssuer(IOptions<JwtOptions> jwt, IOptions<DevAuthOptions> dev, IClock clock)
{
    public DevTokenDto Issue(DevUserOptions user)
    {
        var now = clock.UtcNow;
        var expiresAt = now.AddMinutes(dev.Value.TokenLifetimeMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Value.SigningKey));
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Value.Issuer,
            Audience = jwt.Value.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [IdentityClaims.Subject] = user.Id.ToString(),
                [IdentityClaims.Organisation] = dev.Value.OrganisationId.ToString(),
                [IdentityClaims.Role] = user.Role,
                [IdentityClaims.Name] = user.Name,
            },
        };
        string token = new JsonWebTokenHandler().CreateToken(descriptor);

        return new DevTokenDto(token, expiresAt, user.Id, user.Name, user.Role);
    }
}
