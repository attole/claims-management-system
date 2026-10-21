using System.Text;
using ClaimsModule.API.Errors;
using ClaimsModule.API.Pipeline.Identity;
using ClaimsModule.Application.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ClaimsModule.API.Pipeline.Jwt;

public static class JwtServiceExtensions
{
    private const string ForbiddenMessage = "Your account is not allowed to use this operation.";

    public static IServiceCollection AddJwt(this IServiceCollection services)
    {
        services
            .AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, options) =>
            {
                var jwt = options.Value;
                bearer.MapInboundClaims = false;
                bearer.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.Headers.WWWAuthenticate = JwtBearerDefaults.AuthenticationScheme;

                        return ErrorBody
                            .Unauthorized()
                            .WriteAsync(context.HttpContext, context.HttpContext.RequestAborted);
                    },
                    OnForbidden = context => ErrorBody
                        .Create(ErrorKind.Forbidden, ForbiddenMessage)
                        .WriteAsync(context.HttpContext, context.HttpContext.RequestAborted),
                };
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = IdentityClaims.Name,
                    RoleClaimType = IdentityClaims.Role,
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });

        services
            .AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return services;
    }
}
