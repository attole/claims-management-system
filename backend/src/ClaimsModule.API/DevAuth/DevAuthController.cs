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
    public ActionResult<DevTokenDto> Create(DevTokenRequest request)
    {
        var dev = options.Value;

        if (!dev.Enabled)
        {
            return ErrorBody.Create(ErrorKind.NotFound, NotFoundMessage).ToResult();
        }

        var users = dev.Users.Where(user => user.Id == request.UserId).ToList();

        return users.Count == 0
            ? ErrorBody.Create(ErrorKind.NotFound, NotFoundMessage).ToResult()
            : Ok(issuer.Issue(users[0]));
    }
}
