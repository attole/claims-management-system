using ClaimsModule.Application.Abstractions;
using ClaimsModule.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.API.Tests.Support.TestEndpoints;

[ApiController]
[Route("test")]
public sealed class TestController(
    ICurrentUser user,
    ICorrelationContext correlation,
    ClaimsDbContext context) : ControllerBase
{
    [HttpGet("whoami")]
    public ActionResult<WhoAmIDto> WhoAmI()
    {
        return Ok(new WhoAmIDto(user.Id, user.OrganisationId, user.Role.Marker.ToString(), correlation.CorrelationId));
    }

    [HttpGet("policies")]
    public async Task<ActionResult<int>> CountPolicies(CancellationToken cancellationToken)
    {
        int count = await context.Policies.CountAsync(cancellationToken);

        return Ok(count);
    }

    [AllowAnonymous]
    [HttpGet("anonymous")]
    public IActionResult Anonymous()
    {
        return Ok();
    }
}
