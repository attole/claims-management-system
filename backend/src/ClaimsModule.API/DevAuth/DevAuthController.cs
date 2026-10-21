using System.Security.Cryptography;
using System.Text;
using ClaimsModule.API.DevAuth.Configuration;
using ClaimsModule.API.Errors;
using ClaimsModule.Application.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ClaimsModule.API.DevAuth;

[ApiController]
[AllowAnonymous]
[Route("api/dev/token")]
public sealed class DevAuthController(IOptions<DevAuthOptions> options, DevTokenIssuer issuer) : ControllerBase
{
    private const string NotFoundMessage = "The dev user was not found.";

    [HttpPost]
    [ProducesResponseType<DevTokenDto>(StatusCodes.Status200OK)]
    public ActionResult<DevTokenDto> Create(
        DevTokenRequest request,
        [FromHeader(Name = DevAuthOptions.SecretHeader)] string secret = "")
    {
        var dev = options.Value;

        if (!dev.Enabled)
        {
            return ErrorBody.Create(ErrorKind.NotFound, NotFoundMessage).ToResult();
        }

        if (!IsSecretAccepted(dev, secret))
        {
            return ErrorBody.Create(ErrorKind.Forbidden, "The access code is not valid.").ToResult();
        }

        var users = dev.Users.Where(user => user.Id == request.UserId).ToList();

        return users.Count == 0
            ? ErrorBody.Create(ErrorKind.NotFound, NotFoundMessage).ToResult()
            : Ok(issuer.Issue(users[0]));
    }

    private static bool IsSecretAccepted(DevAuthOptions dev, string secret)
    {
        var expected = SHA256.HashData(Encoding.UTF8.GetBytes(dev.Secret));
        var given = SHA256.HashData(Encoding.UTF8.GetBytes(secret));

        return !dev.HasSecret || CryptographicOperations.FixedTimeEquals(expected, given);
    }
}
