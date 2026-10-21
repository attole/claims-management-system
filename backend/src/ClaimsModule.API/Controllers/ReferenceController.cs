using ClaimsModule.API.Controllers.Requests;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Claims.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/reference")]
public sealed class ReferenceController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet("cause-of-loss-codes")]
    [ProducesResponseType<IReadOnlyList<CauseCodeDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> ListCauseCodes(
        [FromQuery] ListCauseCodesRequest request,
        CancellationToken cancellationToken)
    {
        return QueryAsync(request.ToQuery(), cancellationToken);
    }

    [HttpGet("claim-statuses")]
    [ProducesResponseType<IReadOnlyList<ClaimStatusInfoDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> ListClaimStatuses(CancellationToken cancellationToken)
    {
        return QueryAsync(new ListClaimStatusesQuery(), cancellationToken);
    }
}
