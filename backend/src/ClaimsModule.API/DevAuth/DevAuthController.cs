using ClaimsModule.API.DevAuth.Configuration;
using ClaimsModule.API.DevAuth.Dtos;
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
    public ActionResult<DevTokenDto> Create(DevTokenRequest request)
    {
        var dev = options.Value;
        var users = dev.Users.Where(user => user.Id == request.UserId).ToList();

        if (!dev.Enabled || users.Count == 0)
        {
            return NotFound();
        }

        return Ok(issuer.Issue(users[0]));
    }
}
