using System.Text;
using ClaimsModule.API.DevAuth;
using ClaimsModule.API.DevAuth.Configuration;
using ClaimsModule.API.Pipeline.Identity;
using ClaimsModule.API.Pipeline.Jwt;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Application.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ClaimsModule.API.Tests.DevAuth;

public sealed class DevTokenIssuerTests
{
    private static readonly Guid _organisationId = Guid.Parse("c6000000-0000-4000-8000-000000000001");

    private static readonly DevUserOptions _user = new()
    {
        Id = Guid.Parse("c6000000-0000-4000-8000-000000000002"),
        Name = "Sam Supervisor",
        Role = "Supervisor",
    };

    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    [Fact]
    public async Task Issue_ForAUser_ReturnsATokenTheJwtSettingsAccept()
    {
        var issued = CreateIssuer().Issue(_user);

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(issued.Token, CreateParameters());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Issue_ForAUser_PutsTheIdentityIntoTheClaims()
    {
        var issued = CreateIssuer().Issue(_user);

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(issued.Token, CreateParameters());

        var claims = result.ClaimsIdentity.Claims.ToDictionary(claim => claim.Type, claim => claim.Value);
        claims[IdentityClaims.Subject].Should().Be(_user.Id.ToString());
        claims[IdentityClaims.Organisation].Should().Be(_organisationId.ToString());
        claims[IdentityClaims.Role].Should().Be("Supervisor");
        claims[IdentityClaims.Name].Should().Be("Sam Supervisor");
    }

    [Fact]
    public void Issue_ForAUser_ExpiresAfterTheConfiguredLifetime()
    {
        var issued = CreateIssuer().Issue(_user);

        issued.ExpiresAt.Should().Be(_now.AddMinutes(30));
    }

    [Fact]
    public void Issue_ForAUser_DescribesTheUser()
    {
        var issued = CreateIssuer().Issue(_user);

        issued.Should().BeEquivalentTo(new { UserId = _user.Id, Name = "Sam Supervisor", Role = "Supervisor" });
    }

    private static TokenValidationParameters CreateParameters()
    {
        return new TokenValidationParameters
        {
            ValidIssuer = TestTokens.Issuer,
            ValidAudience = TestTokens.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestTokens.SigningKey)),
        };
    }

    private DevTokenIssuer CreateIssuer()
    {
        var jwt = Options.Create(new JwtOptions
        {
            Issuer = TestTokens.Issuer,
            Audience = TestTokens.Audience,
            SigningKey = TestTokens.SigningKey,
        });
        var dev = Options.Create(new DevAuthOptions
        {
            Enabled = true,
            OrganisationId = _organisationId,
            TokenLifetimeMinutes = 30,
        });
        var clock = new Mock<IClock>();
        clock.SetupGet(mock => mock.UtcNow).Returns(_now);

        return new DevTokenIssuer(jwt, dev, clock.Object);
    }
}
