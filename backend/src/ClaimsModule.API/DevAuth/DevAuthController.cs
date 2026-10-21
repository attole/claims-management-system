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
    [HttpPost]
    [ProducesResponseType<DevTokenDto>(StatusCodes.Status200OK)]
    public ActionResult<DevTokenDto> Create(DevTokenRequest request)
    {
        var dev = options.Value;
        var users = dev.Users.Where(user => user.Id == request.UserId).ToList();

        if (!dev.Enabled || users.Count == 0)
        {
            return ErrorBody.Create(ErrorKind.NotFound, "The dev user was not found.").ToResult();
        }

        return Ok(issuer.Issue(users[0]));
    }
}
