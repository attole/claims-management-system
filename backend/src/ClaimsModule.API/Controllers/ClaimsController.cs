using ClaimsModule.API.Controllers.Requests;
using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Paging;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/claims")]
public sealed class ClaimsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    [ProducesResponseType<PagedResult<ClaimSummaryDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> List([FromQuery] ListClaimsRequest request, CancellationToken cancellationToken)
    {
        return QueryAsync(request.ToQuery(), cancellationToken);
    }
}
