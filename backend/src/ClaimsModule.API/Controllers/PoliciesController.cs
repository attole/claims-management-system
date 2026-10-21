using ClaimsModule.API.Controllers.Requests;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.Policies;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/policies")]
public sealed class PoliciesController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet("search")]
    [ProducesResponseType<PagedResult<PolicyDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> Search([FromQuery] SearchPoliciesRequest request, CancellationToken cancellationToken)
    {
        return QueryAsync(request.ToQuery(), cancellationToken);
    }
}
